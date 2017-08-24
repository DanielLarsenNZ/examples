FROM microsoft/dotnet:latest
COPY . /app
WORKDIR /app
 
RUN ["dotnet", "restore"]
RUN ["dotnet", "build"]
 
EXPOSE 80/tcp
ENV ASPNETCORE_URLS http://*:80
 
ENTRYPOINT ["dotnet", "run"]
