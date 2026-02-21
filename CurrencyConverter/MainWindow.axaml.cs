using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Avalonia.Controls;
using RestSharp;
using Newtonsoft.Json;
using System.Linq;
using Avalonia.Interactivity;

namespace CurrencyConverter;

public partial class MainWindow : Window
{
    Dictionary<string, double> currenciesDict = new Dictionary<string, double>();
    public MainWindow()
    {
        InitializeComponent();

        AmountTxt.GotFocus += AmountTxt_GotFocus!;


        var currencies = GetCurrencies();
        dynamic deserializedCurs = JsonConvert.DeserializeObject(currencies)!; // Deserialize Json into an object

        foreach (var curr in deserializedCurs.data) //Add currency keys to comboboxes
        {
            FromBox.Items.Add(curr.First.code.ToString());
            ToBox.Items.Add(curr.First.code.ToString());
        }
    }

    private string GetCurrencies() // API call to get all currencies
    {
        var client = new RestClient("https://api.currencyapi.com/v3/latest");

        client.Timeout = -1;
        var request = new RestRequest(Method.GET);
        request.AddHeader("apikey", "YOUR_APIKEY");
        IRestResponse response = client.Execute(request);
        return response.Content;

    }
    private void GetCurrencyExchangeRates(string? currencyCode) //API call using specific currency code to get relative exchange rates
    {
        if (currencyCode == null)
        {
            return;
        }

        currenciesDict.Clear(); //Clear dictionary to avoid adding duplicates every calculation

        var client = new RestClient("https://api.currencyapi.com/v3/latest?base_currency=" + currencyCode);

        client.Timeout = -1;
        var request = new RestRequest(Method.GET);
        request.AddHeader("apikey", "YOUR_APIKEY");
        IRestResponse response = client.Execute(request);

        dynamic deserializedCurs = JsonConvert.DeserializeObject(response.Content)!; //Deserialize Json into an object

        foreach (var curr in deserializedCurs.data) //Adding currency codes and exchange rates to dictionary
        {
            currenciesDict.Add(curr.First.code.ToString(), (double)curr.First.value.Value);
        }


    }
    private string calculateResult()
    {
        //Getting input from textfield and comboboxes
        var fromCurrency = FromBox.SelectedItem;
        var toCurrency = ToBox.SelectedItem;
        var amount = Double.Parse(AmountTxt.Text!);

        GetCurrencyExchangeRates(fromCurrency!.ToString());

        //Searching dictionary to find exchange rates
        var fromCurrencyExchange = currenciesDict.First(curr => curr.Key == fromCurrency!.ToString()).Value;
        var toCurrencyExchange = currenciesDict.First(curr => curr.Key == toCurrency!.ToString()).Value;

        var resultAmount = amount * toCurrencyExchange;
        return resultAmount.ToString();
    }
    private void CalculateClick(object? send, RoutedEventArgs rea) //Click method for Calculate button
    {
        ResultTxt.Text = calculateResult();
    }

    private void AmountTxt_GotFocus(object send, RoutedEventArgs rea) //Clearing text when user clicks in textbox
    {
        AmountTxt.Clear();
    }


}