#restore backend
FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build

WORKDIR /src
COPY ./BitbucketPullRequestValidator/BitbucketPullRequestValidator.csproj ./BitbucketPullRequestValidator/
RUN dotnet restore BitbucketPullRequestValidator/BitbucketPullRequestValidator.csproj --source https://api.nuget.org/v3/index.json --source http://proget.spcph.local/nuget/Production/

#build backend
COPY . .
RUN dotnet build "BitbucketPullRequestValidator/BitbucketPullRequestValidator.csproj" -c Release --no-restore -o /app

RUN dotnet publish "BitbucketPullRequestValidator/BitbucketPullRequestValidator.csproj" -c Release --no-restore -o /app

# final stage/image
FROM mcr.microsoft.com/dotnet/core/runtime:3.1

WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "BitbucketPullRequestValidator.dll"]