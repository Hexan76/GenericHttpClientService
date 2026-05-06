/**
 * ============================================================================
 * COREVIA TRANSPORTER - ARCHITECTURE OVERVIEW
 * ============================================================================
 * 
 * This document provides a visual and conceptual overview of the Corevia
 * Transporter architecture.
 * 
 * ============================================================================
 * REQUEST FLOW
 * ============================================================================
 * 
 *  Client Code
 *      ↓
 *  Create Request (HttpRequest, GrpcRequest, DynamicRequest)
 *      ↓
 *  Set Metadata & Properties
 *      ├─ Headers
 *      ├─ Route/Method
 *      ├─ Timeout
 *      └─ Dynamic Properties
 *      ↓
 *  Select Transport (ITransportSelector)
 *      ├─ Per-request preference
 *      ├─ Route-based hints
 *      ├─ Context attributes (feature flags)
 *      └─ Fallback to default
 *      ↓
 *  Get Transport Client (ITransportClientFactory)
 *      ├─ HttpTransportClient
 *      └─ GrpcTransportClient
 *      ↓
 *  Send Request (ITransportClient.SendAsync)
 *      ├─ Apply retry policy
 *      ├─ Handle timeouts
 *      └─ Manage connections
 *      ↓
 *  Receive Response
 *      ├─ HttpResponse, GrpcResponse, DynamicResponse
 *      ├─ Status code
 *      ├─ Headers
 *      ├─ Errors (if any)
 *      └─ Metadata (timing, caching, correlation)
 *      ↓
 *  Process Response
 *      ├─ Check IsSuccess
 *      ├─ Handle errors
 *      ├─ Access typed data
 *      └─ Log/trace via CorrelationId
 * 
 * ============================================================================
 * CLASS HIERARCHY
 * ============================================================================
 * 
 * IRequest
 *  ├─ DynamicRequest : IRequest
 *  │   ├─ DynamicRequest<TResponse>
 *  │   ├─ HttpRequest : DynamicRequest
 *  │   │   └─ HttpRequest<TResponse>
 *  │   └─ GrpcRequest : DynamicRequest
 *  │       └─ GrpcRequest<TResponse>
 *  └─ (Legacy) BaseRequest
 * 
 * IResponse
 *  ├─ DynamicResponse : IResponse
 *  │   ├─ DynamicResponse<TData>
 *  │   ├─ HttpResponse : DynamicResponse
 *  │   │   └─ HttpResponse<TData>
 *  │   └─ GrpcResponse : DynamicResponse
 *  │       └─ GrpcResponse<TData>
 *  └─ (Legacy) BaseResponse
 * 
 * IDynamicObject
 *  └─ Implemented by Request & Response classes
 *      ├─ GetProperty(name)
 *      ├─ SetProperty(name, value)
 *      ├─ HasProperty(name)
 *      └─ GetAllProperties()
 * 
 * ITransportClient
 *  ├─ HttpTransportClient
 *  └─ GrpcTransportClient
 * 
 * ITransportClientFactory
 *  └─ TransportClientFactory
 * 
 * ITransportSelector
 *  └─ DefaultTransportSelector
 * 
 * ============================================================================
 * METADATA STRUCTURE
 * ============================================================================
 * 
 * Request Metadata (IRequestMetadata)
 *  ├─ Headers: Dictionary<string, string>
 *  ├─ Route: string ("/api/users/123")
 *  ├─ Method: string ("GET", "POST", etc.)
 *  ├─ ContentType: string ("application/json", "application/grpc")
 *  ├─ CorrelationId: string (for tracing)
 *  ├─ Timeout: TimeSpan?
 *  ├─ RetryPolicy: RetryPolicy
 *  │   ├─ MaxRetries: int
 *  │   ├─ InitialDelay: TimeSpan
 *  │   ├─ MaxDelay: TimeSpan
 *  │   ├─ BackoffMultiplier: double
 *  │   ├─ RetryableStatusCodes: HashSet<int>
 *  │   └─ RetryableGrpcCodes: HashSet<int>
 *  └─ Attributes: Dictionary<string, object>
 * 
 * Response Metadata (IResponseMetadata)
 *  ├─ StatusCode: int (HTTP or gRPC)
 *  ├─ Headers: Dictionary<string, string>
 *  ├─ ContentType: string
 *  ├─ Message: string
 *  ├─ CorrelationId: string
 *  ├─ Errors: List<ResponseError>
 *  │   ├─ Code: string
 *  │   ├─ Message: string
 *  │   ├─ Details: string
 *  │   ├─ Severity: ErrorSeverity
 *  │   └─ StackTrace: string
 *  ├─ ValidationErrors: List<ValidationError>
 *  │   ├─ PropertyName: string
 *  │   ├─ Message: string
 *  │   ├─ AttemptedValue: object
 *  │   └─ ErrorCode: string
 *  ├─ ElapsedTime: TimeSpan?
 *  ├─ ReceivedAt: DateTime
 *  ├─ IsCached: bool
 *  └─ Attributes: Dictionary<string, object>
 * 
 * ============================================================================
 * TRANSPORT SELECTION FLOW
 * ============================================================================
 * 
 *  ITransportSelector.SelectTransport(request, context)
 *      ↓
 *  Priority 1: Explicit Preference
 *      └─ context.PreferredTransport = "Grpc"
 *      └─ Check if available → return if yes
 *      └─ If unavailable & FailIfPreferred → throw
 *      ↓
 *  Priority 2: Context Attributes
 *      └─ context.Attributes["transport"] = "Http"
 *      └─ Check if available → return if yes
 *      ↓
 *  Priority 3: Route-Based Hints
 *      └─ /grpc/ in route → Grpc
 *      └─ /rest/ or /api/ → Http
 *      ↓
 *  Priority 4: Request Type-Based
 *      └─ GrpcRequest → Grpc
 *      └─ HttpRequest → Http
 *      └─ DynamicRequest → use attribute
 *      ↓
 *  Priority 5: Default Transport
 *      └─ Fallback to configured default (usually Http)
 * 
 * ============================================================================
 * DYNAMIC PROPERTY USAGE
 * ============================================================================
 * 
 * // Setting properties
 * request.SetProperty("UserId", 123);
 * request.SetProperty("FilterActive", true);
 * request.SetProperty("Items", new[] { "a", "b", "c" });
 * 
 * // Getting properties
 * var userId = request.GetProperty("UserId");
 * var items = request.GetProperty("Items");
 * 
 * // Checking existence
 * if (request.HasProperty("UserId"))
 * {
 *     var value = request.GetProperty("UserId");
 * }
 * 
 * // Getting all properties
 * var allProps = request.GetAllProperties();
 * foreach (var kvp in allProps)
 * {
 *     Console.WriteLine($"{kvp.Key}: {kvp.Value}");
 * }
 * 
 * // With strong typing
 * public class UserRequest : HttpRequest<UserResponse>
 * {
 *     public int UserId
 *     {
 *         get => (int?)GetProperty("UserId") ?? 0;
 *         set => SetProperty("UserId", value);
 *     }
 * }
 * 
 * ============================================================================
 * MULTIPLE CONNECTIONS SUPPORT
 * ============================================================================
 * 
 * Named Configurations:
 * 
 *  var options = new TransportOptions
 *  {
 *      ConfigurationName = "production", // vs "staging", "development"
 *      Timeout = TimeSpan.FromSeconds(30)
 *  };
 * 
 *  var response = await client.SendAsync<MyResponse>(request, options);
 * 
 * Benefits:
 *  - Different API endpoints per environment
 *  - Different gRPC servers per region
 *  - Connection pooling per configuration
 *  - Separate retry policies per config
 *  - Load balancing across multiple backends
 * 
 * ============================================================================
 * RICH ERROR HANDLING
 * ============================================================================
 * 
 * Response Errors:
 *  response.Metadata.Errors
 *      ├─ API-level errors
 *      ├─ Code (e.g., "INVALID_PRODUCT")
 *      ├─ Message (user-friendly)
 *      ├─ Details (technical info)
 *      └─ Severity (Information, Warning, Error, Critical)
 * 
 * Validation Errors:
 *  response.Metadata.ValidationErrors
 *      ├─ Field validation failures
 *      ├─ PropertyName (field that failed)
 *      ├─ Message (validation rule description)
 *      ├─ AttemptedValue (what was submitted)
 *      └─ ErrorCode (specific validation rule code)
 * 
 * Usage:
 *  if (!response.IsSuccess)
 *  {
 *      if (response.Metadata.Errors.Any())
 *      {
 *          // Business errors
 *          foreach (var error in response.Metadata.Errors)
 *          {
 *              logger.LogError(error.Code, error.Message);
 *          }
 *      }
 *      
 *      if (response.Metadata.ValidationErrors.Any())
 *      {
 *          // Field validation errors
 *          foreach (var validation in response.Metadata.ValidationErrors)
 *          {
 *              Console.WriteLine($"{validation.PropertyName}: {validation.Message}");
 *          }
 *      }
 *  }
 * 
 * ============================================================================
 * RETRY POLICY CONFIGURATION
 * ============================================================================
 * 
 *  var policy = new RetryPolicy
 *  {
 *      MaxRetries = 3,
 *      InitialDelay = TimeSpan.FromMilliseconds(100),
 *      MaxDelay = TimeSpan.FromSeconds(10),
 *      BackoffMultiplier = 2.0
 *  };
 *  
 *  Delays: 100ms, 200ms, 400ms, 800ms, ...
 *  
 *  Retryable HTTP codes: 408, 429, 500, 502, 503, 504
 *  Retryable gRPC codes: 1, 2, 4, 8, 13, 14
 * 
 * ============================================================================
 * CORRELATION ID TRACING
 * ============================================================================
 * 
 * Set on request:
 *  var correlationId = Guid.NewGuid().ToString();
 *  request.Metadata.CorrelationId = correlationId;
 * 
 * Available in response:
 *  response.Metadata.CorrelationId == correlationId
 * 
 * Usage:
 *  - Log aggregation and distributed tracing
 *  - Request-response correlation
 *  - Cross-service request tracking
 *  - Debugging multi-hop calls
 * 
 * ============================================================================
 */