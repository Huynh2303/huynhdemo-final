using Demo_web_MVC.Models;
using Microsoft.ML;

namespace Demo_web_MVC.Service
{
    public class OrderRiskModelTrainer
    {
        public string Train()
        {
            var mlContext = new MLContext(seed: 1);

            var dataPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "MLData",
                "order_risk_train_synthetic_v5_3000_isvip_medium_fixed.csv"
            );

            var modelFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "MLModels"
            );

            if (!Directory.Exists(modelFolder))
            {
                Directory.CreateDirectory(modelFolder);
            }

            var modelPath = Path.Combine(
                modelFolder,
                "order_risk_model.zip"
            );

            var data = mlContext.Data.LoadFromTextFile<OrderRiskTrainingData>(
                path: dataPath,
                hasHeader: true,
                separatorChar: ','
            );

            // Check nhanh label để tránh lỗi không có class true/false
            var rows = mlContext.Data
                .CreateEnumerable<OrderRiskTrainingData>(
                    data,
                    reuseRowObject: false
                )
                .ToList();

            var normalCount = rows.Count(x => !x.IsRisk);
            var riskCount = rows.Count(x => x.IsRisk);

            if (normalCount == 0 || riskCount == 0)
            {
                return
                    $"Lỗi dữ liệu Label.\n" +
                    $"Bình thường: {normalCount}\n" +
                    $"Bất thường: {riskCount}\n" +
                    $"Kiểm tra lại LoadColumn của IsRisk trong OrderRiskTrainingData.";
            }

            var split = mlContext.Data.TrainTestSplit(
                data,
                testFraction: 0.2
            );

            // Tách riêng bước gom feature
            var dataProcessPipeline = mlContext.Transforms.Concatenate(
                "Features",
                nameof(OrderRiskTrainingData.AccountAgeDays),
                nameof(OrderRiskTrainingData.TotalOrders),
                nameof(OrderRiskTrainingData.OrdersLast24h),
                nameof(OrderRiskTrainingData.OrdersLast7d),
                nameof(OrderRiskTrainingData.CancelledOrders),
                nameof(OrderRiskTrainingData.CancelRate),
                nameof(OrderRiskTrainingData.CurrentOrderValue),
                nameof(OrderRiskTrainingData.AvgOrderValue),
                // nameof(OrderRiskTrainingData.IsCod),
                nameof(OrderRiskTrainingData.CodOrderCount),
                // nameof(OrderRiskTrainingData.PhoneUsedCount),
                // nameof(OrderRiskTrainingData.AddressUsedCount),
                nameof(OrderRiskTrainingData.ItemCount),
                nameof(OrderRiskTrainingData.TotalQuantity),
                // nameof(OrderRiskTrainingData.StatusChangeCount),

                // 3 trường mới
                nameof(OrderRiskTrainingData.IsVip),
                nameof(OrderRiskTrainingData.CompletedOrderCount),
                nameof(OrderRiskTrainingData.CompletionRate)

            // 4 trường đã loại bỏ sau khi test/PFI
            // nameof(OrderRiskTrainingData.CancelledOrdersLast24h),
            // nameof(OrderRiskTrainingData.CancelRateLast24h),
            // nameof(OrderRiskTrainingData.CancelledOrdersLast7d),
            // nameof(OrderRiskTrainingData.CancelRateLast7d)
            );

            var dataProcessModel = dataProcessPipeline.Fit(split.TrainSet);

            var transformedTrainData = dataProcessModel.Transform(split.TrainSet);
            var transformedTestData = dataProcessModel.Transform(split.TestSet);

            var trainer = mlContext.BinaryClassification.Trainers.FastForest(
                labelColumnName: "Label",
                featureColumnName: "Features",
                numberOfLeaves: 20,
                numberOfTrees: 100,
                minimumExampleCountPerLeaf: 10
            );

            // Train FastForest trên dữ liệu đã có cột Features
            var trainedModel = trainer.Fit(transformedTrainData);

            var predictions = trainedModel.Transform(transformedTestData);

            var metrics = mlContext.BinaryClassification.EvaluateNonCalibrated(
                predictions,
                labelColumnName: "Label",
                scoreColumnName: "Score"
            );

            // Chạy PFI trên trainedModel, không chạy trên full pipeline
            var pfi = mlContext.BinaryClassification
                .PermutationFeatureImportanceNonCalibrated(
                    trainedModel,
                    transformedTestData,
                    labelColumnName: "Label",
                    permutationCount: 5
                );

            var pfiResults = pfi
                .Select(metric => new
                {
                    Feature = metric.Key,
                    AucDrop = Math.Abs(metric.Value.AreaUnderRocCurve.Mean),
                    AccuracyDrop = Math.Abs(metric.Value.Accuracy.Mean),
                    F1Drop = Math.Abs(metric.Value.F1Score.Mean)
                })
                .OrderByDescending(x => x.AucDrop)
                .ToList();

            var featureImportanceText = string.Join("\n",
                pfiResults.Select(x =>
                    $"{x.Feature,-35} | AUC Drop: {x.AucDrop:F6} | Accuracy Drop: {x.AccuracyDrop:F6} | F1 Drop: {x.F1Drop:F6}"
                )
            );

            // Ghép lại để lưu model hoàn chỉnh cho lúc predict
            var finalModel = dataProcessModel.Append(trainedModel);

            mlContext.Model.Save(
                finalModel,
                split.TrainSet.Schema,
                modelPath
            );

            var counts = metrics.ConfusionMatrix.Counts;

            var tn = counts[0][0];
            var fn = counts[0][1];
            var fp = counts[1][0];
            var tp = counts[1][1];

            return
                $"Train xong.\n" +
                $"Model: {modelPath}\n\n" +

                $"Tổng dữ liệu:\n" +
                $"Bình thường: {normalCount:N0}\n" +
                $"Bất thường: {riskCount:N0}\n\n" +

                $"Accuracy: {metrics.Accuracy:P2}\n" +
                $"AUC / ROC-AUC: {metrics.AreaUnderRocCurve:P2}\n" +
                $"PR-AUC: {metrics.AreaUnderPrecisionRecallCurve:P2}\n" +
                $"Precision: {metrics.PositivePrecision:P2}\n" +
                $"Recall: {metrics.PositiveRecall:P2}\n" +
                $"F1 Score: {metrics.F1Score:P2}\n\n" +

                $"Confusion Matrix:\n" +
                $"                         Dự đoán bình thường     Dự đoán bất thường\n" +
                $"Thực tế bình thường      {tn,10:N0}              {fp,10:N0}\n" +
                $"Thực tế bất thường       {fn,10:N0}              {tp,10:N0}\n\n" +

                $"TN: {tn:N0} | FP: {fp:N0} | FN: {fn:N0} | TP: {tp:N0}\n\n" +

                $"Feature Importance by PFI:\n" +
                $"{featureImportanceText}";
        }
    }
}