----------------------------------------------------------
# Azure Kubernetes Services (AKS) - Part 06
# AKS based event-driven autoscaling using KEDA

High Level Architecture Diagram:


![Image description](https://github.com/GBuenaflor/01azure-aks-keda/blob/master/Images/GB-AKS-KEDA03.png)



Configuration Flow :

1. Provision the environment using Azure Terraform
2. Create a Azure Function App Container
3. Deploy the container to Dockerhub or Azure Container Registry
4. Deploy KEDA to aks cluster
5. Save data in the Azure Storage Queue, trigger from a web app or console app
6. View the event driven autoscaling using KEDA

Prerequisuite - Environment setup

- Windows 10 EnterpriseN, Verion 1809 ,VM Size [DS2_V3]
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

```
func init --docker
select > dotnet

```


```
func new functionapp01
Select  > queuetrigger
```

### View the function App using VS2019
### Add the Storage Queue Access key to the project, opn local.settings.json file

```
  "azstorageaccnt01_connection": "DefaultEndpointsProtocol=https;AccountName=azstorageaccnt01;AccountKey=s9FysFde5b7D5GbrCWsgyYqLNNxw65xvFqdler10aibcvLC8sL2a0On96wQ/j08gxNSs65mBHpKAQ6nMB/CG6g==;EndpointSuffix=core.windows.net"
```


### In the code add the connection

```
public static void Run([QueueTrigger("myqueue-items", Connection = "azstorageaccnt01_connection")]string myQueueItem, ILogger log)
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
az acr login --name azacr10

# Tag container image
# az acr show --name <acrName> --query loginServer --output table
  az acr show --name azacr10 --query loginServer --output table

docker images

# Tag the weblinux1 image with the loginServer of container registry
# docker tag <Repository> <acrLoginServer>/aci-tutorial-app:v1 
  docker tag fucntionapp01 azacr10.azurecr.io/fucntionapp01:v1


# Push image to Azure Container Registry
# docker push <acrLoginServer>/aci-tutorial-app:v1
  docker push azacr10.azurecr.io/fucntionapp01:v1


# List images in Azure Container Registry
# az acr repository list --name <acrName> --output table
  az acr repository list --name azacr10 --output table

# View the tags for a specific images
# az acr repository show-tags --name <acrName> --repository aci-tutorial-app --output table
  az acr repository show-tags --name azacr10 --repository fucntionapp01 --output table
 
```

----------------------------------------------------------
## 4. Install KEDA to aks cluster and deploy the function app 


### Install KEDA
```
func kubernetes install --namespace keda --validate=false
```

### Deploy the function App using Azure Container Registry
```

func kubernetes deploy --name functionapp01 --registry azacr10.azurecr.io

# Using Docker Hub 
func kubernetes deploy --name functionapp01 --registry gbbuenaflor

```
 
### Deploy the function App using Docker Hub 

```
- Login to Docker Hub
  docker login

- Deploy the function App container
 func kubernetes deploy --name <name-of-function-deployment> --registry <container-registry-username>

```
 
 
### Check the dpeloyment

```
kubectl get deploy
```

----------------------------------------------------------
## 5.  Save data in the Azure Storage Queue, trigger from a web app or console app

### Use the code to save messages in the Azure Queue

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


![Image description](https://github.com/GBuenaflor/01azure-aks-keda/blob/master/Images/GB-AKS-KEDA04.png)

![Image description](https://github.com/GBuenaflor/01azure-aks-keda/blob/master/Images/GB-AKS-KEDA05.png)



----------------------------------------------------------

 
Link to other Microsoft Azure projects
https://github.com/GBuenaflor/01azure
</br>

Note: My Favorite > Microsoft Technologies.
----------------------------------------------------------
