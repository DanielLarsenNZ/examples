# Containers for .NET developers

_North Shore (Auckland) .NET User Group. 24th August 2017_

A closer look at
the options for containerisation from a .NET developer point of view, including
Docker, Containers on Linux and Windows, Azure App Service Containers, Orchestrators
and more.

## Containerisation

> Operating-system-level virtualization, also known as containerization:
<https://en.wikipedia.org/wiki/Containerization>

## Docker

Getting started with Docker.

### Ubuntu + Docker VM

Use your Visual Studio Subscription in Azure and spin up an Ubuntu box with Docker
installed and configured.

Deploy an Ubuntu VM with Docker Engine: <https://azure.microsoft.com/en-us/resources/templates/docker-simple-on-ubuntu/>

Install Ubuntu Shell (and other flavours) on Windows 10 thanks to the **Windows Subsystem
for Linux**: <https://msdn.microsoft.com/en-us/commandline/wsl/install_guide>

    ssh 798db421@52.183.34.50
    docker run hello-world

`hello-world` is an image on **Docker Hub**: <https://hub.docker.com/_/hello-world/>

Like most public images on Docker hub, the **Dockerfile** is open source:
<https://github.com/docker-library/hello-world/blob/master/amd64/hello-world/Dockerfile>

## .NET API in a Container

Create a new .NET Core Web API. Run it.

```bash
dotnet new api -o hello-api
cd hello-api
dotnet run
```

 Open the project in VS Code.

    code .

### Dockerfile

Create a new `Dockerfile`.

```docker
FROM microsoft/dotnet:latest
COPY . /app
WORKDIR /app

RUN ["dotnet", "restore"]
RUN ["dotnet", "build"]

EXPOSE 5000/tcp
ENV ASPNETCORE_URLS http://*:5000

ENTRYPOINT ["dotnet", "run"]
```

### Build and run

```bash
docker build -t webapi4 .
docker run -d -p 5000:5000 -t webapi4
```

<http://localhost:5000/api/values>

### Push

Push your tagged image to the Docker Hub container registry.

    docker ps -a
    docker commit 3f3577516ec8 webapi4
    docker images
    docker tag webapi4 daniellarsennz/webapi4
    docker login
    docker push daniellarsennz/webapi4
    docker images
    docker run -d -p 5000:5000 daniellarsennz/webapi4

Image is now published on public Docker Hub. <https://hub.docker.com/r/daniellarsennz/webapi4/>

You can also push to other public and private registries. **Azure Container
Registry** is a good option. You only pay for the storage:
<https://docs.microsoft.com/en-us/azure/container-registry/>

## Linux App Service Containers

App Services for Linux containers are great for hosting Docker containers:
<https://docs.microsoft.com/en-us/azure/app-service-web/app-service-linux-using-custom-docker-image>

The best bit is you get an SSL offloading reverse-proxy "for free". The app
service will map the exposed port to 80 and 443 (via a proxy - I'm pretty sure it's
nginx under the hood!). Use the *.azurewebsites.net wildcard cert or bring your own.

## Docker for Windows

Run Linux and Windows daemons on Windows: <https://docs.docker.com/docker-for-windows/>

## Visual Studio

Visual Studio support for containers is getting better every release. The trick is
to keep Visual Studio up to date. This is almost a daily occurance at the moment :/

* Install VS2017 (latest)
* VS2017 Extensions and Updates > Updates > Update everything
* Install Docker for Windows (latest)
* Install .NET Core 2.0 <https://www.microsoft.com/net/core#windowscmd>
* Restart VS2017

### ASP.NET Core Web API

* New > ASP.NET Core Web Application
* Web API, .NET Core 2.0, Linux OS
* F5 build, breakpoints and debug on localhost

## Windows Server containers

**Windows Server 2016** has Windows Server Container support. You can also use Docker for
Windows. Two Windows Container base images: **Windows Server Core** and **Windows Nano
Server**. You can run .NET Framework ASP.NET applications on Windows Nano Server:
<https://docs.microsoft.com/en-us/windows-server/get-started/iis-on-nano-server>

## Azure Container instances

These are crazy cool. There is an Azure CLI in the Azure Portal (in the browser)
now. Try this:

    az container create --name webapi4 --image daniellarsennz/webapi4 --resource-group hello-aci-rg --port 5000 --ip-address public

Instructions: <https://docs.microsoft.com/en-us/azure/container-instances/container-instances-quickstart>

## Scaling containers

**Docker compose** files describe how to deploy multiple containers as a set of services:
<https://docs.docker.com/compose/overview/>

Microsoft's latest thinking on .NET Core Container Microservices architecture:
<https://github.com/dotnet-architecture/eShopOnContainers>. The ebook is really good.

### Orchestrators

Once you scale to more than a handful of containers you need an Orchestrator.

**Kubernetes** on **Azure Container Service**: <https://docs.microsoft.com/en-us/azure/container-service/kubernetes/>

Consider **Service Fabric** for an alternative to Container Orchestrators. Works well
with .NET: <https://docs.microsoft.com/en-us/azure/service-fabric/>
