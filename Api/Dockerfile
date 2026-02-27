FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore dependencies (separate layer — only reruns if .csproj changes)
COPY *.csproj .
RUN dotnet restore

# Build and publish
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Railway injects PORT at runtime — ASPNETCORE_HTTP_PORTS picks it up
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# Run as non-root user (included in the base image)
USER app

ENTRYPOINT ["dotnet", "Api.dll"]
