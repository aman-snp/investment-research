FROM 524234064850.dkr.ecr.us-east-1.amazonaws.com/base-dev/rhel-ubi8/dotnet-31 AS buildEnv
WORKDIR ./sourceCode
COPY . .
RUN ls
RUN dotnet restore ./SPGMI.Actors.InvestmentResearch.ResearchIndexer/SPGMI.Actors.InvestmentResearch.ResearchIndexer.csproj --configfile ./nuget.config
# -o paramter value of publish command uses absolute path so that publishOutput folder is in root, using relative path would create it relative to the csproj folder.
RUN dotnet publish --no-restore -c Release -o /publishOutput ./SPGMI.Actors.InvestmentResearch.ResearchIndexer/SPGMI.Actors.InvestmentResearch.ResearchIndexer.csproj

 #02 Pipeline Pre-requisites
FROM 524234064850.dkr.ecr.us-east-1.amazonaws.com/datapipeline-core:pipeline-base-rhel-ubi8-latest as pipeline-base-image

FROM 524234064850.dkr.ecr.us-east-1.amazonaws.com/base-dev/rhel-ubi8/dotnet-31-runtime

#Config Service [Environment specific]
#Dev: configUrl.dev.xml 
#Test: configUrl.test.xml
#Staging: configUrl.stg.xml
#Prod 
    #CHO:   configUrl.prod_cho.xml
    #QTS:   configUrl.prod_qts.xml
    #EWDC:  configUrl.prod_ewdc.xml
COPY --from=pipeline-base-image /capitalIQ/configuration /capitalIQ/configuration
ENV CAPITALIQ_CONFIGURATION "/capitalIQ/configuration/configUrl.dev.xml"


#Hermes libs (with kafka native libraries)
COPY --from=pipeline-base-image /hermes/lib/netstandard2.0 /hermes/lib/netstandard2.0
ENV HERMES_ROOT "/hermes/lib/netstandard2.0"

WORKDIR ./app
COPY --from=buildEnv /publishOutput .

#ARG HOST_ENV=Development
#ENV ASPNETCORE_ENVIRONMENT ${HOST_ENV}
ENV DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER 0


RUN ls
EXPOSE 80
ENTRYPOINT ["dotnet", "SPGMI.Actors.InvestmentResearch.ResearchIndexer.dll"]