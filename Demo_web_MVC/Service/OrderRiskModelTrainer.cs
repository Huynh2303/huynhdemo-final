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
                "order_risk_train_synthetic_v4_3000.csv"
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

            var split = mlContext.Data.TrainTestSplit(
                data,
                testFraction: 0.2
            );

            var pipeline = mlContext.Transforms.Concatenate(
                    "Features",
                    nameof(OrderRiskTrainingData.AccountAgeDays),
                    nameof(OrderRiskTrainingData.TotalOrders),
                    nameof(OrderRiskTrainingData.OrdersLast24h),
                    nameof(OrderRiskTrainingData.OrdersLast7d),
                    nameof(OrderRiskTrainingData.CancelledOrders),
                    nameof(OrderRiskTrainingData.CancelRate),
                    nameof(OrderRiskTrainingData.CurrentOrderValue),
                    nameof(OrderRiskTrainingData.AvgOrderValue),
                    nameof(OrderRiskTrainingData.IsCod),
                    nameof(OrderRiskTrainingData.CodOrderCount),
                    nameof(OrderRiskTrainingData.PhoneUsedCount),
                    nameof(OrderRiskTrainingData.AddressUsedCount),
                    nameof(OrderRiskTrainingData.ItemCount),
                    nameof(OrderRiskTrainingData.TotalQuantity),
                    nameof(OrderRiskTrainingData.StatusChangeCount),
                    nameof(OrderRiskTrainingData.CancelledOrdersLast24h),
                    nameof(OrderRiskTrainingData.CancelRateLast24h),
                    nameof(OrderRiskTrainingData.CancelledOrdersLast7d),
                    nameof(OrderRiskTrainingData.CancelRateLast7d)
                )
                .Append(mlContext.BinaryClassification.Trainers.FastForest(
                    labelColumnName: "Label",
                    featureColumnName: "Features",
                    numberOfLeaves: 20,
                    numberOfTrees: 100,
                    minimumExampleCountPerLeaf: 10
                ));

            var model = pipeline.Fit(split.TrainSet);

            var predictions = model.Transform(split.TestSet);

            var metrics = mlContext.BinaryClassification.EvaluateNonCalibrated(
                predictions,
                labelColumnName: "Label",
                scoreColumnName: "Score"
            );

            mlContext.Model.Save(model, split.TrainSet.Schema, modelPath);

            var counts = metrics.ConfusionMatrix.Counts;

            var tn = counts[0][0]; // Thực tế bình thường, dự đoán bình thường
            var fp = counts[0][1]; // Thực tế bình thường, dự đoán bất thường
            var fn = counts[1][0]; // Thực tế bất thường, dự đoán bình thường
            var tp = counts[1][1]; // Thực tế bất thường, dự đoán bất thường

            return
                $"Train xong.\n" +
                $"Model: {modelPath}\n\n" +

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

                $"TN: {tn:N0} | FP: {fp:N0} | FN: {fn:N0} | TP: {tp:N0}";
        }
    }
}