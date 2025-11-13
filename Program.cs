using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;



//Added Program class  with main function as a entry point of the program
public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("--- Sales Report Generator ---");

        // Inicializing year and month to 0 and get the values from the user in terminal
        int targetYear = 0;
        int targetMonth = 0;

        while (targetYear == 0)
        {
            Console.Write("Please enter the year for the report: ");
            string yearInput = Console.ReadLine();

            if (int.TryParse(yearInput, out int year) && year > 1900 && year <= DateTime.Now.Year + 1)
            {
                targetYear = year;
            }
            else
            {
                Console.WriteLine("Invalid year. Please enter a valid four-digit year.");
            }
        }
        while (targetMonth == 0)
        {
            Console.Write("Please enter the month for the report (1-12): ");
            string monthInput = Console.ReadLine();

            if (int.TryParse(monthInput, out int month) && month >= 1 && month <= 12)
            {
                targetMonth = month;
            }
            else
            {
                Console.WriteLine("Invalid month. Please enter a number between 1 and 12.");
            }
        }
        //Add a try-catch to detect errrors on reportService
        try
        {
            var reportService = new ReportService();
            reportService.CreateReportForYearAndMonth(targetYear, targetMonth);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
        }
    }
}

//Using an enum for the currencies, possible to expand to  extra currencies in the future
public enum Currency
{
    USD,
    EUR,
    GBP,
}

// SaleObject using Currency enum for Currency
public class SaleObject
{
    public DateTime Date { get; set; }
    public double Amount { get; set; }
    public Currency Currency { get; set; }
}


public class ReportService
{
    // Conversion rates and documents as private constants
    private const double EUR_TO_USD_RATE = 1.1;
    private const double GBP_TO_USD_RATE = 1.3;
    private const string SALES_FILE = "sales.json";
    private const string REPORT_FILE_FORMAT = "report_{0}_{1}.txt"; // {0}=year, {1}=month


    public void CreateReportForYearAndMonth(int year, int month)
    {
        // 1 Load Data
        List<SaleObject> allSales = LoadSalesData(SALES_FILE);

        // 2 Filter Data
        var filteredSales = allSales
            .Where(s => s.Date.Year == year && s.Date.Month == month)
            .ToList();

        // 3 Aggregate Data to USD
        double totalUSD = CalculateTotalUSD(filteredSales);

        // 4 Generate Report
        GenerateReportFile(year, month, totalUSD);

        Console.WriteLine("Bericht erfolgreich erstellt.");
    }

    // Helper Methods
    
    // Isolates file I/O and deserialization logic
    private List<SaleObject> LoadSalesData(string filePath)
    {
        //try and catch in case we do not find the file  or orhter exception occur 
        try
        {
            string jsonContent = File.ReadAllText(filePath);
            //if the file  has not meaningful content it return an emphy list
            if (string.IsNullOrWhiteSpace(jsonContent))
                return new List<SaleObject>();


        //options  for the Json seliralization to convert the strings into enums for the currency field
         var options = new JsonSerializerOptions
        {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },  
        };
            //Deserialize  jsonContent. if it returns null we return an emphy list
            return JsonSerializer.Deserialize<List<SaleObject>>(jsonContent,options) ?? new List<SaleObject>();
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"Error: Sales file '{filePath}' not found.");
            return new List<SaleObject>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading sales data: {ex.Message}");
            return new List<SaleObject>();
        }
    }

    // Agregate all the sales in USD
    private double CalculateTotalUSD(List<SaleObject> sales)
    {
        double total = 0;
        foreach (var sale in sales)
        {
            total += ConvertToUSD(sale.Amount, sale.Currency);
        }
        return total;
    }

    // Currency conversion to USD 
    private double ConvertToUSD(double amount, Currency currency)
    {
        switch (currency)
        {
            case Currency.USD:
                return amount;
            case Currency.EUR:
                return amount * EUR_TO_USD_RATE;
            case Currency.GBP:
                return amount * GBP_TO_USD_RATE;
            default:
                return 0;
        }
    }

    // Report output in console
    private void GenerateReportFile(int year, int month, double total)
    {
        var lines = new List<string>
        {
            $"Monatlicher Verkaufsbericht {month}/{year}",
            "-------------------------------------",
            $"Gesamt Umsatz in USD: {total}" 
        };

        string fileName = string.Format(REPORT_FILE_FORMAT, year, month);
        File.WriteAllLines(fileName, lines);
    }
}