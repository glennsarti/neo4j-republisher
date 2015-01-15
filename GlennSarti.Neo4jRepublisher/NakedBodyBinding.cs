// FROM http://weblog.west-wind.com/posts/2013/Dec/13/Accepting-Raw-Request-Body-Content-with-ASPNET-Web-API
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using GlennSarti.Neo4jRepublisher;

namespace System.Web.Http
{
  /// <summary>
  /// An attribute that captures the entire content body and stores it
  /// into the parameter of type string or byte[].
  /// </summary>
  /// <remarks>
  /// The parameter marked up with this attribute should be the only parameter as it reads the
  /// entire request body and assigns it to that parameter.
  /// </remarks>
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
  public sealed class NakedBodyAttribute : ParameterBindingAttribute
  {
    public override HttpParameterBinding GetBinding(HttpParameterDescriptor parameter)
    {
      if (parameter == null)
        throw new ArgumentException("Invalid parameter");
      return new NakedBodyParameterBinding(parameter);
    }
  }
}

namespace GlennSarti.Neo4jRepublisher
{
  public class EmptyTask
  {
    public static Task Start()
    {
      var taskSource = new TaskCompletionSource<AsyncVoid>();
      taskSource.SetResult(default(AsyncVoid));
      return taskSource.Task as Task;
    }
    private struct AsyncVoid
    {
    }
  }
  
  /// <summary>
  /// Reads the Request body into a string/byte[] and
  /// assigns it to the parameter bound.
  ///
  /// Should only be used with a single parameter on
  /// a Web API method using the [NakedBody] attribute
  /// </summary>
  public class NakedBodyParameterBinding : HttpParameterBinding
  {
    public NakedBodyParameterBinding(HttpParameterDescriptor descriptor)
      : base(descriptor)
    {
    }
    /// <summary>
    /// Check for simple
    /// </summary>
    /// <param name="metadataProvider"></param>
    /// <param name="actionContext"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override Task ExecuteBindingAsync(ModelMetadataProvider metadataProvider,
    HttpActionContext actionContext,
    CancellationToken cancellationToken)
    {
      var binding = actionContext
      .ActionDescriptor
      .ActionBinding;
      if (binding.ParameterBindings.Length > 1 ||
      actionContext.Request.Method == HttpMethod.Get)
        return EmptyTask.Start();
      var type = binding
      .ParameterBindings[0]
      .Descriptor.ParameterType;
      if (type == typeof(string))
      {
        return actionContext.Request.Content
        .ReadAsStringAsync()
        .ContinueWith((task) =>
        {
          var stringResult = task.Result;
          SetValue(actionContext, stringResult);
        });
      }
      else if (type == typeof(byte[]))
      {
        return actionContext.Request.Content
        .ReadAsByteArrayAsync()
        .ContinueWith((task) =>
        {
          byte[] result = task.Result;
          SetValue(actionContext, result);
        });
      }
      throw new InvalidOperationException("Only string and byte[] are supported for [NakedBody] parameters");
    }
    public override bool WillReadBody
    {
      get
      {
        return true;
      }
    }
  }
}