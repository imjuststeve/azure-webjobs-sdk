using System;
using System.Reflection;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Microsoft.WindowsAzure.Jobs.Azure20SdkBinders
{
    // Providers binders for Azure 2.0 types. These are in different assemblies than 1.*
    //  2.0 is in Microsoft.WindowsAzure.Storage.dll
    //  1.7 is in Microsoft.WindowsAzure.StorageClient.dll
    internal class Azure20SdkBinderProvider : ICloudBinderProvider
    {
        public ICloudBinder TryGetBinder(Type targetType)
        {
            if (Utility.IsAzureSdk20Type(targetType))
            {
                return TryGetBinderWorker(targetType);
            }
            return null;
        }

        private ICloudBinder TryGetBinderWorker(Type targetType)
        {
            if (targetType == typeof(CloudStorageAccount))
            {
                return new CloudStorageAccountBinder();
            }
            if (targetType == typeof(CloudQueue))
            {
                return new CloudQueueBinder();
            }            
            return null;
        }
    }

    class CloudQueueBinder : ICloudBinder, ICloudBinderVerify
    {
        public BindResult Bind(IBinderEx bindingContext, ParameterInfo parameter)
        {
            return new BindResult { Result = GetCloudQueue(bindingContext, parameter) };
        }

        private static CloudQueue GetCloudQueue(IBinderEx bindingContext, ParameterInfo parameter)
        {
            CloudStorageAccount account = bindingContext.GetAccount();

            // How to get q-name? Or other info from the attributes.
            string queueName = parameter.Name;
                
            var q = account.CreateCloudQueueClient().GetQueueReference(queueName);
            q.CreateIfNotExists();
            return q;
        }

        void ICloudBinderVerify.Validate(ParameterInfo parameter)
        {
            string queueName = parameter.Name;
            Microsoft.WindowsAzure.Jobs.Utility.ValidateQueueName(queueName);
        }
    }

    class CloudStorageAccountBinder : ICloudBinder
    {
        public BindResult Bind(IBinderEx bindingContext, ParameterInfo parameter)
        {                
            return new BindResult
            {
                Result = bindingContext.GetAccount()
            };
        }
    }
    
}