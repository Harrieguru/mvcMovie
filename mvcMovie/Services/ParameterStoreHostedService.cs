using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using System.Threading;
using System.Collections.Generic;
using System;

namespace mvcMovie.Services
{
    public class ParameterStoreHostedService : IHostedService
    {
        private readonly MyCustomConfiguration _customConfig;
        private readonly IAmazonSimpleSystemsManagement _ssmClient;

        public ParameterStoreHostedService(MyCustomConfiguration customConfig, IAmazonSimpleSystemsManagement ssmClient)
        {
            _customConfig = customConfig ?? throw new ArgumentNullException(nameof(customConfig));
            _ssmClient = ssmClient ?? throw new ArgumentNullException(nameof(ssmClient));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                // Replace these with your actual Parameter Store keys
                var parameterNames = new List<string>
                {
                    "/config/movieapp/AWS/Region",
                    "/config/movieapp/Connection2RDS",
                    "/config/movieapp/AWS/AccessKey",
                    "/config/movieapp/AWS/SecretKey",
                    "/config/movieapp/AllowedHosts"
                };

                var request = new GetParametersRequest
                {
                    Names = parameterNames,
                    WithDecryption = true
                };

                var response = await _ssmClient.GetParametersAsync(request, cancellationToken);
                foreach (var parameter in response.Parameters)
                {
                    // Assume the Name corresponds directly to a property on MyCustomConfiguration
     
                    switch (parameter.Name)
                    {
                        case "/config/movieapp/AWS/Region":
                            _customConfig.AWSRegion = parameter.Value;
                            break;
                        case "/config/movieapp/Connection2RDS":
                            _customConfig.Connection2RDS = parameter.Value;
                            break;
                        case "/config/movieapp/AWS/AccessKey":
                            _customConfig.AWSAccessKey = parameter.Value;
                            break;
                        case "/config/movieapp/AWS/SecretKey":
                            _customConfig.AWSSecretKey = parameter.Value;
                            break;
                        case "/config/movieapp/AllowedHosts":
                            _customConfig.AllowedHosts = parameter.Value;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception as needed
                throw; // Or handle it as needed
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // Implement any cleanup tasks here if necessary
            return Task.CompletedTask;
        }
    }
}
