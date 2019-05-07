# Hello nginx

## Load balancer

Load balance two (or more) App Services with nginx running in a Linux App Service
container. The App Service will also bind custom domains and offload SSL if required.

    lb/azuredeploy.json
    lb/Dockerfile
    lb/nginx.conf

`nginx.conf` creates the proxies and virtual servers required to set the host headers
that App Services needs for application request routing. It is based on this example:
<http://www.gigasacs.net/language/en/2016/05/19/load-balancing-two-azure-webapp-with-nginx/>

`Dockerfile` copies the custom `nginx.conf` into the official nginx container.

`azuredeploy.json` deploys a Linux App Service Plan and App Service that hosts the
customised nginx container.

### Deployment

> Tip: Use VSTS to automate the build and deployment of the container

Build, run and test the container

    cd lb
    docker build -t nginx-lb .
    docker run -p 127.0.0.1:80:80 -d nginx-lb
    curl http://127.0.0.1

Commit, tag and push the container

    docker ps   # (find the image id for nginx-lb)
    docker commit 26cfca7280c0 hello-nginx-lb
    docker tag hello-nginx-lb daniellarsennz/hello-nginx-lb
    docker push daniellarsennz/hello-nginx-lb

Deploy the App Service Plan and App Service (Azure PowerShell)

```powershell
New-AzureRmResourceGroupDeployment -Name "hello-nginx-$(New-Guid)" `
    -ResourceGroupName hello-nginx-rg -TemplateFile .\azuredeploy.json `
    -siteName hello-nginx -hostingPlanName hello-nginx-plan `
    -containerImageName daniellarsennz/hello-nginx-lb -Verbose
```

GET the web app URL to warm up the site and run the container

    curl https://hello-nginx.azurewebsites.net

### Why?

Why load balance App Services when the service already load-balances across instances?
One scenario is scaling to greater than 20 App Services instances (across multiple App
Service Plans). The other scenario is when advanced load-balancing features are
required.

### More info

Nginx directive reference: <https://nginx.org/en/docs/dirindex.html>

Nginx core module reference: <https://nginx.org/en/docs/ngx_core_module.html>

Docker push: <https://docs.docker.com/engine/reference/commandline/push/>

Load balancing two Azure WebAPP with nginx: <http://www.gigasacs.net/language/en/2016/05/19/load-balancing-two-azure-webapp-with-nginx/>
