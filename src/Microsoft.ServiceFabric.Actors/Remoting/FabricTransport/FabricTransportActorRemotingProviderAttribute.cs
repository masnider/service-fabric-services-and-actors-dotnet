﻿// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------
namespace Microsoft.ServiceFabric.Actors.Remoting.FabricTransport
{
    using System;
    using Microsoft.ServiceFabric.Actors.Remoting.FabricTransport.Runtime;
    using Microsoft.ServiceFabric.Actors.Runtime;
    using Microsoft.ServiceFabric.Services.Remoting;
    using Microsoft.ServiceFabric.Services.Remoting.Client;
    using Microsoft.ServiceFabric.Services.Remoting.FabricTransport;
    using Microsoft.ServiceFabric.Services.Remoting.Runtime;
    /// <summary>
    ///     Sets fabric TCP transport as the default remoting provider for the actors.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class FabricTransportActorRemotingProviderAttribute : ActorRemotingProviderAttribute
    {
        /// <summary>
        /// Instantiates a new <see cref="FabricTransportActorRemotingProviderAttribute"/>, which can be used to set 
        /// fabric TCP transport as the default remoting provider for the actors.
        /// </summary>
        public FabricTransportActorRemotingProviderAttribute()
        {
        }

        /// <summary>
        /// Gets or Sets the maximum size of the remoting message in bytes.
        /// If value for this property is not specified or it is less than or equals to zero,
        /// a default value of 4,194,304 bytes (4 MB) is used.
        /// </summary>
        /// <value>
        ///     The maximum size of the remoting message in bytes. If this value is not specified 
        ///     or it is less than or equals to zero, a default value of 4,194,304 bytes (4 MB) is used.
        /// </value>
        public long MaxMessageSize { get; set; }

        /// <summary>
        ///     Gets or Sets the operation timeout in seconds. If the operation is not completed in the specified
        ///     time, it will be timed out. By default, exception handler of 
        ///     <see cref="Microsoft.ServiceFabric.Services.Remoting.FabricTransport.Client.FabricTransportServiceRemotingClientFactory"/>
        ///     retries the timed out exception. It is recommended to not change the operation timeout from it's default value. 
        /// </summary>
        /// <value>
        ///     The operation timeout in seconds. If not specified or less than zero, default operation timeout
        ///     of maximum value is used. 
        /// </value>
        public long OperationTimeoutInSeconds { get; set; }

        /// <summary>
        ///     Gets or Sets the keep alive timeout in seconds. This settings is useful in the scenario when the client 
        ///     and service are connected via load balancer that closes the connection if it is idle for some time.
        ///     If keep alive timeout is configured, the connection will be kept alive by sending ping messages at 
        ///     that interval.
        /// </summary>
        /// <value>
        ///     The keep alive timeout in seconds.
        /// </value>
        public long KeepAliveTimeoutInSeconds { get; set; }

        /// <summary>
        ///     Gets or Sets the connect timeout in milliseconds. This settings specifies the maximum time allowed for the connection 
        ///     to be established.
        /// </summary>
        /// <value>
        ///     The connect timeout in Milliseconds.
        /// </value>
        /// <remarks>Default Value for ConnectTimeout Timeout is 5 seconds.</remarks>
        public long ConnectTimeoutInMilliseconds { get; set; }

        /// <summary>
        ///     Creates a service remoting listener for remoting the actor interfaces.
        /// </summary>
        /// <param name="actorService">
        ///     The implementation of the actor service that hosts the actors whose interfaces
        ///     needs to be remoted.
        /// </param>
        /// <returns>
        ///     A <see cref="Microsoft.ServiceFabric.Services.Remoting.FabricTransport.Runtime.FabricTransportServiceRemotingListener"/>
        ///     as <see cref="Microsoft.ServiceFabric.Services.Remoting.Runtime.IServiceRemotingListener"/> 
        ///     for the specified actor service.
        /// </returns>
        public override IServiceRemotingListener CreateServiceRemotingListener(ActorService actorService)
        {
            var listenerSettings = FabricTransportActorServiceRemotingListener.GetActorListenerSettings(actorService);
            listenerSettings.MaxMessageSize = this.GetAndValidateMaxMessageSize(listenerSettings.MaxMessageSize);
            listenerSettings.OperationTimeout = this.GetandValidateOperationTimeout(listenerSettings.OperationTimeout);
            listenerSettings.KeepAliveTimeout = this.GetandValidateKeepAliveTimeout(listenerSettings.KeepAliveTimeout);
            return new FabricTransportActorServiceRemotingListener(actorService, listenerSettings);
        }

        /// <summary>
        ///     Creates a service remoting client factory to connect to the remoted actor interfaces.
        /// </summary>
        /// <param name="callbackClient">
        ///     Client implementation where the callbacks should be dispatched.
        /// </param>
        /// <returns>
        ///     A <see cref="Microsoft.ServiceFabric.Actors.Remoting.FabricTransport.FabricTransportActorRemotingClientFactory "/>
        ///     as <see cref="Microsoft.ServiceFabric.Services.Remoting.Client.IServiceRemotingClientFactory"/>
        ///     that can be used with <see cref="Microsoft.ServiceFabric.Actors.Client.ActorProxyFactory"/> to 
        ///     generate actor proxy to talk to the actor over remoted actor interface.
        /// </returns>
        public override IServiceRemotingClientFactory CreateServiceRemotingClientFactory(
            IServiceRemotingCallbackClient callbackClient)
        {
            var settings = FabricTransportRemotingSettings.GetDefault();
            settings.MaxMessageSize = this.GetAndValidateMaxMessageSize(settings.MaxMessageSize);
            settings.OperationTimeout = this.GetandValidateOperationTimeout(settings.OperationTimeout);
            settings.KeepAliveTimeout = this.GetandValidateKeepAliveTimeout(settings.KeepAliveTimeout);
            settings.ConnectTimeout = this.GetConnectTimeout(settings.ConnectTimeout);
            return new FabricTransportActorRemotingClientFactory(settings, callbackClient);
        }

        private long GetAndValidateMaxMessageSize(long maxMessageSize)
        {
            return (this.MaxMessageSize > 0) ? this.MaxMessageSize : maxMessageSize;
        }

        private TimeSpan GetandValidateOperationTimeout(TimeSpan operationTimeout)
        {
            return (this.OperationTimeoutInSeconds > 0)
                ? TimeSpan.FromSeconds(this.OperationTimeoutInSeconds)
                : operationTimeout;
        }

        private TimeSpan GetandValidateKeepAliveTimeout(TimeSpan keepAliveTimeout)
        {
            return (this.KeepAliveTimeoutInSeconds > 0)
                ? TimeSpan.FromSeconds(this.KeepAliveTimeoutInSeconds)
                : keepAliveTimeout;
        }

        private TimeSpan GetConnectTimeout(TimeSpan connectTimeout)
        {
            return (this.ConnectTimeoutInMilliseconds > 0)
                ? TimeSpan.FromMilliseconds(this.ConnectTimeoutInMilliseconds)
                : connectTimeout;
        }
    }
}
