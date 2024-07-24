﻿using CSharpFunctionalExtensions;
using Hng.Application.Features.PaymentIntegrations.Paystack.Dtos.Common;
using Hng.Application.Features.PaymentIntegrations.Paystack.Dtos.Requests;
using Hng.Application.Features.PaymentIntegrations.Paystack.Dtos.Responses;
using Newtonsoft.Json;
using System.Text;
using System.Text.Json;

namespace Hng.Application.Features.PaymentIntegrations.Paystack.Services
{
    public class PaystackClient : IPaystackClient
    {
        private const string Authorization = nameof(Authorization);
        private const string Bearer = nameof(Bearer);
        private const string MediaType = "application/json";
        private readonly HttpClient _client;

        private readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = new LowerCaseNamingPolicy()
        };

        public static class Endpoints
        {
            public static string VerifyTransfer => "transfer/verify/{0}";
            public static string TransactionInitialize => "transaction/initialize";
        }

        public PaystackClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<Result<VerifyTransferResponse>> VerifyTransfer(VerifyTransactionRequest request)
            => await FetchFromPaystack<VerifyTransferResponse, VerifyTransactionRequest>(request, Endpoints.VerifyTransfer);

        public async Task<Result<InitializeTransactionResponse>> InitializeTransaction(InitializeTransactionRequest request)
    => await SendToPaystack<InitializeTransactionResponse, InitializeTransactionRequest>(request, Endpoints.TransactionInitialize);

        private async Task<Result<TResponse>> SendToPaystack<TResponse, TRequest>(TRequest request, string endpoint) where TRequest : PaymentRequestBase
        {
            ArgumentNullException.ThrowIfNull(request.BusinessAuthorizationToken);
            var authorizedClient = SetAuthToken(_client, request.BusinessAuthorizationToken);
            try
            {
                var serializedRequest = System.Text.Json.JsonSerializer.Serialize(request, SerializerOptions);
                var body = new StringContent(serializedRequest, Encoding.UTF8, MediaType);
                var httpResponse = await authorizedClient.PostAsync(endpoint, body);


                if (!httpResponse.IsSuccessStatusCode)
                    return Result.Failure<TResponse>(await httpResponse.Content.ReadAsStringAsync());

    //            var response2 =
    //System.Text.Json.JsonSerializer.Deserialize< Dictionary < String, dynamic>> (await httpResponse.Content.ReadAsStringAsync());
                
                var response =
                    JsonConvert.DeserializeObject<TResponse>(await httpResponse.Content.ReadAsStringAsync());
                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<TResponse>(ex.Message);
            }
        }

        private async Task<Result<U>> FetchFromPaystack<U, T>(T requestParam, string endpoint) where T : PaymentQueryBase<string>
        {
            var authorizedClient = SetAuthToken(_client, requestParam.BusinessAuthorizationToken);
            try
            {
                var requestPath = string.Format(endpoint, requestParam.Param);
                var httpResponse = await authorizedClient.GetAsync(requestPath);

                if (!httpResponse.IsSuccessStatusCode)
                    return Result.Failure<U>(await httpResponse.Content.ReadAsStringAsync());
                var response =
                    System.Text.Json.JsonSerializer.Deserialize<U>(await httpResponse.Content.ReadAsStringAsync(), SerializerOptions);
                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<U>(ex.Message);
            }
        }

        private static HttpClient SetAuthToken(HttpClient client, string businessAuthorizationToken)
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add(Authorization, $"{Bearer} {businessAuthorizationToken}");
            return client;
        }
    }
}