----------------------------------------------------------
# Azure Kubernetes Services (AKS) - Part 07
# AKS based event-driven autoscaling using KEDA with Function App Docker Container 

#### High Level Architecture Diagram:


![Image description](https://github.com/GBuenaflor/01azure-aks-keda/blob/master/Images/GB-AKS-KEDA03.png)



#### Configuration Flow :

1. Provision the environment using Azure Terraform
2. Create a Azure Function App Container
3. Deploy the container to Dockerhub or Azure Container Registry
4. Deploy KEDA to aks cluster
5. Save data in the Azure Storage Queue, trigger from a web app or console app
6. View the event driven autoscaling using KEDA

#### Prerequisite - Environment setup

- Install Docker for Windows
  https://docs.docker.com/docker-for-windows/install/  
- Install VS2019 Community Edition
  https://visualstudio.microsoft.com/vs/compare/
- Install NodeJS
  https://nodejs.org/en/download/

----------------------------------------------------------
## 1. Provision the environment using Azure Terraform

```
terraform init
terraform plan
terraform apply
```

### Connect to AKS

```
# Get K8S Credentials
az aks get-credentials --resource-group Env-aks-KEDA-RG --name az-k8s

# Enable addOn
az aks enable-addons --addons kube-dashboard --resource-group Env-aks-KEDA-RG --name az-k8s

# Open Kubernetes Dash Board
az aks browse --resource-group Env-aks-KEDA-RG --name az-k8s
   

kubectl get nodes
```

----------------------------------------------------------
## 2. Create a Azure Function App Container

### Open a commandline, run as Administrator

```
func init --docker
select > dotnet

```

```
func new functionapp01
Select  > queuetrigger
```

### Add the Azure Storage Queue Access key to local.settings.json file from the FunctionApp project.

```
  "azstorageaccnt01_connection": "DefaultEndpointsProtocol=https;AccountName=azstorageaccnt01;AccountKey=s9FysFde5b7D5GbrCWsgyYqLNNxw65xvFqdler10aibcvLC8sL2a0On96wQ/j08gxNSs65mBHpKAQ6nMB/CG6g==;EndpointSuffix=core.windows.net"
```


### In the code, update the the QueueTrigger details from functionapp01.cs file. The queue name must match the queue name created by the terraform

```
public static void Run([QueueTrigger("az-strorage-queue01", Connection = "azstorageaccnt01_connection")]string myQueueItem, ILogger log)
```

### View the docker file

```
FROM microsoft/dotnet:2.2-sdk AS installer-env

COPY . /src/dotnet-function-app
RUN cd /src/dotnet-function-app && \
    mkdir -p /home/site/wwwroot && \
    dotnet publish *.csproj --output /home/site/wwwroot

# To enable ssh & remote debugging on app service change the base image to the one below
# FROM mcr.microsoft.com/azure-functions/dotnet:2.0-appservice 
FROM mcr.microsoft.com/azure-functions/dotnet:2.0
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

COPY --from=installer-env ["/home/site/wwwroot", "/home/site/wwwroot"]
```  

### Build the functionApp Container

```
  docker login
  docker build -t functionapp01 .
```

----------------------------------------------------------
## 3. Deploy the container to Azure Container Registry

### Using Azure Container Registry
```
# Login to ACR
  az acr login --name azacr10

# Tag container image
  az acr show --name azacr10 --query loginServer --output table

docker images

# Tag the weblinux1 image with the loginServer of container registry
  docker tag fucntionapp01 azacr10.azurecr.io/fucntionapp01:v1


# Push image to Azure Container Registry
  docker push azacr10.azurecr.io/fucntionapp01:v1

# List images in Azure Container Registry
  az acr repository list --name azacr10 --output table

# View the tags for a specific images
  az acr repository show-tags --name azacr10 --repository fucntionapp01 --output table
 
```

----------------------------------------------------------
## 4. Install KEDA to aks cluster and deploy the function app container


### Install KEDA
```
func kubernetes install --namespace keda --validate=false
```

### Deploy the function App from Azure Container Registry to aks
```
- Login to ACR
 az acr login --name azacr10
 
# func kubernetes deploy --name <name-of-function-deployment> --registry <acr-login-server>
  func kubernetes deploy --name functionapp01 --registry azacr10.azurecr.io
```
 
 
### Deploy the function App from DockerHub to aks 

```
- Login to Docker Hub
  docker login

- Deploy the function App container
# func kubernetes deploy --name <name-of-function-deployment> --registry <container-registry-username>
  func kubernetes deploy --name functionapp01 --registry gbbuenaflor

```
 
 
### Check the deployment

```
kubectl get deploy
```

----------------------------------------------------------
## 5.  Save data in the Azure Storage Queue, trigger from a web app or console app

### Use the code to save messages in the Azure Queue
#### - In Web App, you normally save shopping cart data to the queue
#### - In Console App, you can iterate and save data to the queue manually

```
 string azstorageaccnt01_connection = "DefaultEndpointsProtocol=https;AccountName=azstorageaccnt01;AccountKey=s9FysFde5b7D5GbrCWsgyYqLNNxw65xvFqdler10aibcvLC8sL2a0On96wQ/j08gxNSs65mBHpKAQ6nMB/CG6g==;EndpointSuffix=core.windows.net";

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(azstorageaccnt01_connection);
            CloudQueueClient cloudQueueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue cloudQueue = cloudQueueClient.GetQueueReference("az-strorage-queue01"); //name of storage queue

            // Line below for Testing only , In Web App, you normally save shopping cart data to the queue
            for (int i = 0; i < 30000; i++)
            {
                CloudQueueMessage queueMessage = new CloudQueueMessage("New Data: " + System.DateTime.Now.ToString()); //message to write in the queue

                cloudQueue.AddMessage(queueMessage);
                Console.WriteLine("Message:" + i.ToString());

            }

```

----------------------------------------------------------
## 6. View the event driven autoscaling using KEDA


### View the function app deployment using acr 

![Image description](https://github.com/GBuenaflor/01azure-aks-keda/blob/master/Images/GB-AKS-KEDA04.png)

### View the function app deployment using dockerhub

![Image description](https://github.com/GBuenaflor/01azure-aks-keda/blob/master/Images/GB-AKS-KEDA05.png)



### View the autoscaling, the number of pods and number of deployments within aks

### Scale Up

![Image description](https://github.com/GBuenaflor/01azure-aks-keda/blob/master/Images/GB-AKS-KEDA06.png)


### Scale Down

![Image description](https://github.com/GBuenaflor/01azure-aks-keda/blob/master/Images/GB-AKS-KEDA07.png)


----------------------------------------------------------

 
Link to other Microsoft Azure projects
https://github.com/GBuenaflor/01azure
</br>

Note: My Favorite > Microsoft Technologies.
----------------------------------------------------------
