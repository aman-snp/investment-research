FROM 524234064850.dkr.ecr.us-east-1.amazonaws.com/datapipeline-core:pipeline-base-rhel-ubi8-latest 

#{pwd}: artifacts published directory
#copy app artifacts from {pwd}
COPY . /app
WORKDIR /app

ENTRYPOINT ["dotnet", "SPGMI.Actors.InvestmentResearch.ResearchIndexResponseTracker.dll"]
