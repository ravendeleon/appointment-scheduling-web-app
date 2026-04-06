FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# copy project file and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# use the smaller runtime image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# copy the built app from the build stage
COPY --from=build /app/out ./

# expose port 8080 for Railway
EXPOSE 8080

# set the entry point
ENTRYPOINT ["dotnet", "SchedulingApp.dll"]