using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

class Program
{
    static void Main()
    {
        try
        {
            string inputFilePath = "input.json";
            string outputFilePath = "output.txt";

            JObject operationsData = ReadJsonFile(inputFilePath);

            List<KeyValuePair<string, double>> results = ProcessOperations(operationsData);

            WriteResults(results, outputFilePath);
        }
        catch (FileNotFoundException e)
        {
            Console.WriteLine($"Error: {e.Message}");
        }
        catch (ArgumentException e)
        {
            Console.WriteLine($"Invalid argument: {e.Message}");
        }
    }

    static List<KeyValuePair<string, double>> ProcessOperations(JObject operationsData)
    {
        if (!operationsData.Properties().Any())
        {
            throw new ArgumentException("The JSON object is empty.");
        }
        List<KeyValuePair<string, double>> results = new List<KeyValuePair<string, double>>();

        foreach (var operation in operationsData)
        {
            string objectName = operation.Key;
            if (operation.Value is JObject content)
            {
                MathOperation mathOperation = ParseOperation(content);
                double result = PerformOperation(
                    mathOperation.OperationType,
                    mathOperation.Value1,
                    mathOperation.Value2
                );

                results.Add(new KeyValuePair<string, double>(objectName, result));
            }
            else
            {
                throw new ArgumentException("Invalid data in operation");
            }
        }
        results.Sort((x, y) => x.Value.CompareTo(y.Value));
        return results;
    }

    static JObject ReadJsonFile(string inputFile)
    {
        if (!File.Exists(inputFile))
        {
            throw new FileNotFoundException($"File not found: {inputFile}");
        }

        using (StreamReader reader = new(inputFile))
        {
            string jsonData = reader.ReadToEnd().Trim();

            if (string.IsNullOrEmpty(jsonData))
            {
                throw new ArgumentException("The file has no content");
            }

            try
            {
                return JObject.Parse(jsonData);
            }
            catch (JsonReaderException)
            {
                throw new ArgumentException("The file contains invalid JSON");
            }
        }
    }

    static MathOperation ParseOperation(JObject operationData)
    {
        if (!operationData.ContainsKey("operator"))
            throw new ArgumentException("Missing 'operator' field in the operation.");

        if (!operationData.ContainsKey("value1"))
            throw new ArgumentException("Missing 'value1' field in the operation.");

        string? operatorType = operationData.Value<string>("operator");
        if (string.IsNullOrWhiteSpace(operatorType))
        {
            throw new ArgumentException("The 'operator' field is empty or contains invalid data.");
        }

        double value1 = operationData.Value<double>("value1");
        double value2 = 0;

        if (operatorType != "sqrt")
        {
            if (!operationData.ContainsKey("value2"))
                throw new ArgumentException(
                    "Missing 'value2' field in the operation for operator: "
                );

            value2 = operationData.Value<double>("value2");
        }

        return new MathOperation(operatorType, value1, value2);
    }

    static double PerformOperation(string operationType, double value1, double value2)
    {
        switch (operationType)
        {
            case "add":
                return value1 + value2;
            case "sub":
                return value1 - value2;
            case "mul":
                return value1 * value2;
            case "sqrt":
                if (value1 < 0)
                {
                    throw new ArgumentException(
                        $"Cannot compute square root of negative number: {value1}"
                    );
                }
                return Math.Sqrt(value1);
            default:
                throw new ArgumentException($"Unsupported operation type: {operationType}");
        }
    }

    static void WriteResults(List<KeyValuePair<string, double>> results, string outputFile)
    {
        try
        {
            using (StreamWriter writer = new(outputFile))
            {
                foreach (var result in results)
                {
                    writer.WriteLine($"{result.Key}: {result.Value:F2}");
                }
            }

            Console.WriteLine("Results written to output.txt");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing to file: {ex.Message}");
        }
    }
}

class MathOperation(string operationType, double value1, double value2)
{
    public string OperationType { get; } = operationType;
    public double Value1 { get; } = value1;
    public double Value2 { get; } = value2;
}
