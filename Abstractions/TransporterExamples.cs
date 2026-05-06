//namespace Corevia.Transporter.Abstractions;

///// <summary>
///// Example implementations and extension methods for the Corevia Transporter system
///// 
///// This file demonstrates how to:
///// 1. Create requests with the new dynamic model system
///// 2. Send requests across different transports
///// 3. Handle responses with rich metadata
///// 4. Implement per-request transport selection
///// 5. Configure retry policies and other options
///// </summary>
//public static class TransporterExamples
//{
//    // ============================================================================
//    // EXAMPLE 1: Using HTTP Transport with Dynamic Requests
//    // ============================================================================

//    public static class HttpTransportExample
//    {
//        public class GetUserRequest : HttpRequest<GetUserResponse>
//        {
//            public GetUserRequest(int userId)
//            {
//                Route = $"/api/users/{userId}";
//                HttpMethod = System.Net.Http.HttpMethod.Get;
//                SetProperty("UserId", userId);
//            }
//        }

//        public class GetUserResponse : HttpResponse<User>
//        {
//        }

//        public class User
//        {
//            public int Id { get; set; }
//            public string Name { get; set; } = "";
//            public string Email { get; set; } = "";
//        }

//        // Usage example
//        public static async Task ExampleUsage(ITransportClient httpClient)
//        {
//            var request = new GetUserRequest(userId: 123);

//            // Add custom headers
//            request.Metadata.Headers["Authorization"] = "Bearer token";

//            // Set timeout
//            var options = new TransportOptions
//            {
//                Timeout = TimeSpan.FromSeconds(30),
//                CorrelationId = Guid.NewGuid().ToString()
//            };

//            var response = await httpClient.SendAsync<GetUserResponse>(request, options);

//            if (response.IsSuccess)
//            {
//                Console.WriteLine($"User: {response.Data?.Name}");
//            }
//            else
//            {
//                Console.WriteLine($"Error: {response.Metadata.Message}");
//                foreach (var error in response.Metadata.Errors)
//                {
//                    Console.WriteLine($"  - {error.Code}: {error.Message}");
//                }
//            }
//        }
//    }

//    // ============================================================================
//    // EXAMPLE 2: Using gRPC Transport
//    // ============================================================================

//    public static class GrpcTransportExample
//    {
//        public class GetProductRequest : GrpcRequest<GetProductResponse>
//        {
//            public GetProductRequest(string productId)
//            {
//                ServiceName = "ProductService";
//                MethodName = "GetProduct";
//                SetProperty("ProductId", productId);
//            }
//        }

//        public class GetProductResponse : GrpcResponse<Product>
//        {
//        }

//        public class Product
//        {
//            public string Id { get; set; } = "";
//            public string Name { get; set; } = "";
//            public decimal Price { get; set; }
//        }

//        // Usage example
//        public static async Task ExampleUsage(ITransportClient grpcClient)
//        {
//            var request = new GetProductRequest(productId: "PROD-001");

//            var options = new TransportOptions
//            {
//                ConfigurationName = "production",
//                Timeout = TimeSpan.FromSeconds(60)
//            };

//            var response = await grpcClient.SendAsync<GetProductResponse>(request, options);

//            if (response.IsSuccess && response.Data != null)
//            {
//                Console.WriteLine($"Product: {response.Data.Name} - ${response.Data.Price}");
//            }
//        }
//    }

//    // ============================================================================
//    // EXAMPLE 3: Per-Request Transport Selection
//    // ============================================================================

//    public static class TransportSelectionExample
//    {
//        public class OrderRequest : DynamicRequest<OrderResponse>
//        {
//            public OrderRequest(string orderId)
//            {
//                Route = $"/api/orders/{orderId}";
//                Metadata.Method = "GET";
//                SetProperty("OrderId", orderId);
//            }
//        }

//        public class OrderResponse : DynamicResponse<Order>
//        {
//        }

//        public class Order
//        {
//            public string Id { get; set; } = "";
//            public DateTime CreatedAt { get; set; }
//            public decimal Total { get; set; }
//        }

//        // Usage example with transport selection
//        public static async Task ExampleUsage(
//            ITransportClientFactory factory,
//            ITransportSelector selector)
//        {
//            var request = new OrderRequest(orderId: "ORD-123");

//            // Determine which transport to use
//            var context = new TransportSelectionContext
//            {
//                UserId = "user-456",
//                PreferredTransport = null, // Will use selector logic
//                Attributes = 
//                {
//                    ["useGrpc"] = true,  // Feature flag
//                    ["region"] = "us-west"
//                }
//            };

//            var transportName = selector.SelectTransport(request, context);
//            var transport = factory.GetClient(transportName);

//            if (transport != null)
//            {
//                var response = await transport.SendAsync<OrderResponse>(request);
//                Console.WriteLine($"Transport used: {transport.TransportName}");
//                Console.WriteLine($"Order total: ${response.Data?.Total}");
//            }
//        }
//    }

//    // ============================================================================
//    // EXAMPLE 4: Dynamic Request with Retry Policy
//    // ============================================================================

//    public static class RetryPolicyExample
//    {
//        public class CreatePaymentRequest : HttpRequest<CreatePaymentResponse>
//        {
//            public CreatePaymentRequest(decimal amount, string currency = "USD")
//            {
//                Route = "/api/payments";
//                HttpMethod = System.Net.Http.HttpMethod.Post;
//                SetProperty("Amount", amount);
//                SetProperty("Currency", currency);
//            }
//        }

//        public class CreatePaymentResponse : HttpResponse<PaymentResult>
//        {
//        }

//        public class PaymentResult
//        {
//            public string TransactionId { get; set; } = "";
//            public string Status { get; set; } = "";
//        }

//        // Usage example with retry policy
//        public static async Task ExampleUsage(ITransportClient httpClient)
//        {
//            var request = new CreatePaymentRequest(amount: 99.99, currency: "USD");

//            // Configure retry policy
//            var retryPolicy = new RetryPolicy
//            {
//                MaxRetries = 3,
//                InitialDelay = TimeSpan.FromMilliseconds(100),
//                MaxDelay = TimeSpan.FromSeconds(10),
//                BackoffMultiplier = 2.0
//            };

//            var options = new TransportOptions
//            {
//                Timeout = TimeSpan.FromSeconds(30),
//                RetryPolicy = retryPolicy,
//                CorrelationId = Guid.NewGuid().ToString()
//            };

//            var response = await httpClient.SendAsync<CreatePaymentResponse>(request, options);

//            if (response.IsSuccess)
//            {
//                Console.WriteLine($"Payment successful: {response.Data?.TransactionId}");
//            }
//            else
//            {
//                // Check validation errors
//                if (response.Metadata.ValidationErrors.Any())
//                {
//                    foreach (var error in response.Metadata.ValidationErrors)
//                    {
//                        Console.WriteLine($"Validation: {error.PropertyName} - {error.Message}");
//                    }
//                }
//            }
//        }
//    }

//    // ============================================================================
//    // EXAMPLE 5: Dependency Injection Setup
//    // ============================================================================

//    public static class DependencyInjectionExample
//    {
//        // Configure in your DI container
//        public static void ConfigureTransporters(/*IServiceCollection services*/)
//        {
//            // Example using Microsoft.Extensions.DependencyInjection
//            /*
//            services.AddScoped<IHttpClientFactory, HttpClientFactory>();
//            services.AddScoped<IHttpClientService, HttpClientService>();
            
//            // HTTP Transport
//            services.AddScoped(sp => 
//                new HttpTransportClient(sp.GetRequiredService<IHttpClientService>())
//            );

//            // gRPC Transport
//            services.AddScoped(sp => 
//                new GrpcTransportClient(new GrpcClientServiceOptions 
//                {
//                    DefaultAddress = "https://grpc-server:50051",
//                    EnableTls = true
//                })
//            );

//            // Transport Factory
//            services.AddScoped<ITransportClientFactory>(sp =>
//            {
//                var factory = new TransportClientFactory();
//                factory.RegisterClient("Http", sp.GetRequiredService<HttpTransportClient>());
//                factory.RegisterClient("Grpc", sp.GetRequiredService<GrpcTransportClient>());
//                factory.SetDefaultTransport("Http");
//                return factory;
//            });

//            // Transport Selector
//            services.AddScoped<ITransportSelector>(sp =>
//            {
//                var factory = sp.GetRequiredService<ITransportClientFactory>();
//                var selector = new DefaultTransportSelector(factory);
//                selector.SetDefaultTransport("Http");
//                return selector;
//            });
//            */
//        }
//    }

//    // ============================================================================
//    // EXAMPLE 6: Custom Request/Response Types
//    // ============================================================================

//    public static class CustomTypesExample
//    {
//        // Custom request with business logic
//        public class BatchImportRequest : DynamicRequest
//        {
//            public string FilePath
//            {
//                get => (string)GetProperty("FilePath") ?? "";
//                set => SetProperty("FilePath", value);
//            }

//            public int BatchSize
//            {
//                get => (int?)GetProperty("BatchSize") ?? 100;
//                set => SetProperty("BatchSize", value);
//            }

//            public bool ValidateOnly
//            {
//                get => (bool?)GetProperty("ValidateOnly") ?? false;
//                set => SetProperty("ValidateOnly", value);
//            }

//            public BatchImportRequest()
//            {
//                Metadata.Route = "/api/batch/import";
//                Metadata.Method = "POST";
//                Metadata.Timeout = TimeSpan.FromMinutes(5); // Long timeout for batch operations
//            }
//        }

//        public class BatchImportResponse : DynamicResponse
//        {
//            public int SuccessCount => (int?)GetProperty("SuccessCount") ?? 0;
//            public int FailureCount => (int?)GetProperty("FailureCount") ?? 0;
//            public List<string> FailureDetails => (List<string>)GetProperty("FailureDetails") ?? new();
//        }

//        // Usage
//        public static async Task ExampleUsage(ITransportClient client)
//        {
//            var request = new BatchImportRequest
//            {
//                FilePath = "/uploads/data.csv",
//                BatchSize = 500,
//                ValidateOnly = false
//            };

//            var response = await client.SendAsync(
//                request,
//                typeof(BatchImportResponse),
//                new TransportOptions { Timeout = TimeSpan.FromMinutes(5) }
//            ) as BatchImportResponse;

//            if (response != null && response.IsSuccess)
//            {
//                Console.WriteLine($"Imported: {response.SuccessCount}, Failed: {response.FailureCount}");
//            }
//        }
//    }
//}
//namespace Corevia.Transporter.Abstractions;

///// <summary>
///// Example implementations and extension methods for the Corevia Transporter system
///// 
///// This file demonstrates how to:
///// 1. Create requests with the new dynamic model system
///// 2. Send requests across different transports
///// 3. Handle responses with rich metadata
///// 4. Implement per-request transport selection
///// 5. Configure retry policies and other options
///// </summary>
//public static class TransporterExamples
//{
//    // ============================================================================
//    // EXAMPLE 1: Using HTTP Transport with Dynamic Requests
//    // ============================================================================

//    public static class HttpTransportExample
//    {
//        public class GetUserRequest : HttpRequest<GetUserResponse>
//        {
//            public GetUserRequest(int userId)
//            {
//                Route = $"/api/users/{userId}";
//                HttpMethod = System.Net.Http.HttpMethod.Get;
//                SetProperty("UserId", userId);
//            }
//        }

//        public class GetUserResponse : HttpResponse<User>
//        {
//        }

//        public class User
//        {
//            public int Id { get; set; }
//            public string Name { get; set; } = "";
//            public string Email { get; set; } = "";
//        }

//        // Usage example
//        public static async Task ExampleUsage(ITransportClient httpClient)
//        {
//            var request = new GetUserRequest(userId: 123);

//            // Add custom headers
//            request.Metadata.Headers["Authorization"] = "Bearer token";

//            // Set timeout
//            var options = new TransportOptions
//            {
//                Timeout = TimeSpan.FromSeconds(30),
//                CorrelationId = Guid.NewGuid().ToString()
//            };

//            var response = await httpClient.SendAsync<GetUserResponse>(request, options);

//            if (response.IsSuccess)
//            {
//                Console.WriteLine($"User: {response.Data?.Name}");
//            }
//            else
//            {
//                Console.WriteLine($"Error: {response.Metadata.Message}");
//                foreach (var error in response.Metadata.Errors)
//                {
//                    Console.WriteLine($"  - {error.Code}: {error.Message}");
//                }
//            }
//        }
//    }

//    // ============================================================================
//    // EXAMPLE 2: Using gRPC Transport
//    // ============================================================================

//    public static class GrpcTransportExample
//    {
//        public class GetProductRequest : GrpcRequest<GetProductResponse>
//        {
//            public GetProductRequest(string productId)
//            {
//                ServiceName = "ProductService";
//                MethodName = "GetProduct";
//                SetProperty("ProductId", productId);
//            }
//        }

//        public class GetProductResponse : GrpcResponse<Product>
//        {
//        }

//        public class Product
//        {
//            public string Id { get; set; } = "";
//            public string Name { get; set; } = "";
//            public decimal Price { get; set; }
//        }

//        // Usage example
//        public static async Task ExampleUsage(ITransportClient grpcClient)
//        {
//            var request = new GetProductRequest(productId: "PROD-001");

//            var options = new TransportOptions
//            {
//                ConfigurationName = "production",
//                Timeout = TimeSpan.FromSeconds(60)
//            };

//            var response = await grpcClient.SendAsync<GetProductResponse>(request, options);

//            if (response.IsSuccess && response.Data != null)
//            {
//                Console.WriteLine($"Product: {response.Data.Name} - ${response.Data.Price}");
//            }
//        }
//    }

//    // ============================================================================
//    // EXAMPLE 3: Per-Request Transport Selection
//    // ============================================================================

//    public static class TransportSelectionExample
//    {
//        public class OrderRequest : DynamicRequest<OrderResponse>
//        {
//            public OrderRequest(string orderId)
//            {
//                Route = $"/api/orders/{orderId}";
//                Metadata.Method = "GET";
//                SetProperty("OrderId", orderId);
//            }
//        }

//        public class OrderResponse : DynamicResponse<Order>
//        {
//        }

//        public class Order
//        {
//            public string Id { get; set; } = "";
//            public DateTime CreatedAt { get; set; }
//            public decimal Total { get; set; }
//        }

//        // Usage example with transport selection
//        public static async Task ExampleUsage(
//            ITransportClientFactory factory,
//            ITransportSelector selector)
//        {
//            var request = new OrderRequest(orderId: "ORD-123");

//            // Determine which transport to use
//            var context = new TransportSelectionContext
//            {
//                UserId = "user-456",
//                PreferredTransport = null, // Will use selector logic
//                Attributes = 
//                {
//                    ["useGrpc"] = true,  // Feature flag
//                    ["region"] = "us-west"
//                }
//            };

//            var transportName = selector.SelectTransport(request, context);
//            var transport = factory.GetClient(transportName);

//            if (transport != null)
//            {
//                var response = await transport.SendAsync<OrderResponse>(request);
//                Console.WriteLine($"Transport used: {transport.TransportName}");
//                Console.WriteLine($"Order total: ${response.Data?.Total}");
//            }
//        }
//    }

//    // ============================================================================
//    // EXAMPLE 4: Dynamic Request with Retry Policy
//    // ============================================================================

//    public static class RetryPolicyExample
//    {
//        public class CreatePaymentRequest : HttpRequest<CreatePaymentResponse>
//        {
//            public CreatePaymentRequest(decimal amount, string currency = "USD")
//            {
//                Route = "/api/payments";
//                HttpMethod = System.Net.Http.HttpMethod.Post;
//                SetProperty("Amount", amount);
//                SetProperty("Currency", currency);
//            }
//        }

//        public class CreatePaymentResponse : HttpResponse<PaymentResult>
//        {
//        }

//        public class PaymentResult
//        {
//            public string TransactionId { get; set; } = "";
//            public string Status { get; set; } = "";
//        }

//        // Usage example with retry policy
//        public static async Task ExampleUsage(ITransportClient httpClient)
//        {
//            var request = new CreatePaymentRequest(amount: 99.99, currency: "USD");

//            // Configure retry policy
//            var retryPolicy = new RetryPolicy
//            {
//                MaxRetries = 3,
//                InitialDelay = TimeSpan.FromMilliseconds(100),
//                MaxDelay = TimeSpan.FromSeconds(10),
//                BackoffMultiplier = 2.0
//            };

//            var options = new TransportOptions
//            {
//                Timeout = TimeSpan.FromSeconds(30),
//                RetryPolicy = retryPolicy,
//                CorrelationId = Guid.NewGuid().ToString()
//            };

//            var response = await httpClient.SendAsync<CreatePaymentResponse>(request, options);

//            if (response.IsSuccess)
//            {
//                Console.WriteLine($"Payment successful: {response.Data?.TransactionId}");
//            }
//            else
//            {
//                // Check validation errors
//                if (response.Metadata.ValidationErrors.Any())
//                {
//                    foreach (var error in response.Metadata.ValidationErrors)
//                    {
//                        Console.WriteLine($"Validation: {error.PropertyName} - {error.Message}");
//                    }
//                }
//            }
//        }
//    }

//    // ============================================================================
//    // EXAMPLE 5: Dependency Injection Setup
//    // ============================================================================

//    public static class DependencyInjectionExample
//    {
//        // Configure in your DI container
//        public static void ConfigureTransporters(/*IServiceCollection services*/)
//        {
//            // Example using Microsoft.Extensions.DependencyInjection
//            /*
//            services.AddScoped<IHttpClientFactory, HttpClientFactory>();
//            services.AddScoped<IHttpClientService, HttpClientService>();
            
//            // HTTP Transport
//            services.AddScoped(sp => 
//                new HttpTransportClient(sp.GetRequiredService<IHttpClientService>())
//            );

//            // gRPC Transport
//            services.AddScoped(sp => 
//                new GrpcTransportClient(new GrpcClientServiceOptions 
//                {
//                    DefaultAddress = "https://grpc-server:50051",
//                    EnableTls = true
//                })
//            );

//            // Transport Factory
//            services.AddScoped<ITransportClientFactory>(sp =>
//            {
//                var factory = new TransportClientFactory();
//                factory.RegisterClient("Http", sp.GetRequiredService<HttpTransportClient>());
//                factory.RegisterClient("Grpc", sp.GetRequiredService<GrpcTransportClient>());
//                factory.SetDefaultTransport("Http");
//                return factory;
//            });

//            // Transport Selector
//            services.AddScoped<ITransportSelector>(sp =>
//            {
//                var factory = sp.GetRequiredService<ITransportClientFactory>();
//                var selector = new DefaultTransportSelector(factory);
//                selector.SetDefaultTransport("Http");
//                return selector;
//            });
//            */
//        }
//    }

//    // ============================================================================
//    // EXAMPLE 6: Custom Request/Response Types
//    // ============================================================================

//    public static class CustomTypesExample
//    {
//        // Custom request with business logic
//        public class BatchImportRequest : DynamicRequest
//        {
//            public string FilePath
//            {
//                get => (string)GetProperty("FilePath") ?? "";
//                set => SetProperty("FilePath", value);
//            }

//            public int BatchSize
//            {
//                get => (int?)GetProperty("BatchSize") ?? 100;
//                set => SetProperty("BatchSize", value);
//            }

//            public bool ValidateOnly
//            {
//                get => (bool?)GetProperty("ValidateOnly") ?? false;
//                set => SetProperty("ValidateOnly", value);
//            }

//            public BatchImportRequest()
//            {
//                Metadata.Route = "/api/batch/import";
//                Metadata.Method = "POST";
//                Metadata.Timeout = TimeSpan.FromMinutes(5); // Long timeout for batch operations
//            }
//        }

//        public class BatchImportResponse : DynamicResponse
//        {
//            public int SuccessCount => (int?)GetProperty("SuccessCount") ?? 0;
//            public int FailureCount => (int?)GetProperty("FailureCount") ?? 0;
//            public List<string> FailureDetails => (List<string>)GetProperty("FailureDetails") ?? new();
//        }

//        // Usage
//        public static async Task ExampleUsage(ITransportClient client)
//        {
//            var request = new BatchImportRequest
//            {
//                FilePath = "/uploads/data.csv",
//                BatchSize = 500,
//                ValidateOnly = false
//            };

//            var response = await client.SendAsync(
//                request,
//                typeof(BatchImportResponse),
//                new TransportOptions { Timeout = TimeSpan.FromMinutes(5) }
//            ) as BatchImportResponse;

//            if (response != null && response.IsSuccess)
//            {
//                Console.WriteLine($"Imported: {response.SuccessCount}, Failed: {response.FailureCount}");
//            }
//        }
//    }
//}
