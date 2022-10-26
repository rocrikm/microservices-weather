# microservices-weather
What is it and how can you use it in AWS.
We'll build four separate applications from scratch in .NET, containerize them with Docker, and orchestrate them in the cloud using AWS Fargate with ECS.  Each of our services will work with its own database (database-per-service), and will communicate over HTTP.

If you're following along, it will be useful to have Docker installed on your local development environment.  Additionally, having pgAdmin installed will enable you to connect to both local and RDS-hosted PostgreSQL databases.

Each microservice has its own project in the solution.
Each microservice has an endpoint defined in program.cs for get and put. 

DataLoader microservice that calls into the Temperature and Precipitation microservice has an appSettings file (don’t forget the properties file to set to always load) that contains the service, host and port definitions for the precipitation and temperature services. In program.cs initialize httpclients for each microservice using uri definition based on appSettings and calls into the microservice endpoint using a postAsync methof

WeatherReport has a BusinessLogic class that aggregates the data from the other microservices. 
It builds the httpclient access to the precipitation and temperature microservices access based on the appSettings info - ideally would go through a service discovery service.  Reads from DB using getAsync methods accessing the microservices get endpoint.

Will use EFCore.

In order to deploy in cloud we need to dockerize the microservices. Each microservice will get a dockerfile. Test locally before moving to AWS.

AWS Environment
All you need is a free account to AWS and free tier resources for this exercise. 
Set up a VPC where you create everything - virtual private cloud - an isolated location in the cloud with 2 availability zones, 2 public subnets with resources that can interact with the greater public internet and other that will not interact with the outside world. The NAT Gateway enables outside communication - outbound only. Need to have DNS hostnames and DNS Resolution Enabled otherwise the RDS creation will not work
Set up ECR repository for each service. Create ECR(Elastic Container Registry) for each microservice with default settings.

push to AWS ECR by running the file for each service { NOTE - your domain will be different so change it in your files}
    ./build_and_push_ecr_image.sh
[if it doesn’t work change mode to run using chmod +x build_and_push_ecr_iamge.sh]

Set up ECS - Fargate (Elastic Container Service)
[An Amazon ECS cluster is a logical grouping of tasks or services. Your tasks and services are run on infrastructure that is registered to a cluster. The infrastructure capacity can be provided by AWS Fargate, which is serverless infrastructure that AWS manages, Amazon EC2 instances that you manage, or an on-premise server or virtual machine (VM) that you manage remotely.]

- first create a cluster (network only) - weather-demo
- In the cluster we create a task definition for each of the services we want to run. We’re using Fargate again because we’re not using it for EC2 generation as internal support or as external based on an on-prem infrastructure.

“A task definition specifies which containers are included in your task and how they interact with each other. You can also specify data volumes for your containers to use.”
Containers will use the images that we have just uploaded in ECR.
IMPORTANT: The containers will use Environment Variables which will overwrite the settings in appsettings.json files in the corresponding microservice.  These are injected at runtime using a naming convention like this:

<<Section Name>>__(this is double underscore)<<Variable Name>>
e.g.: CONNECTIONSTRINGS__APPDB

- Create a PostGres resource in Amazon RDS so we can specify the corresponding DB for the container connection.  weather-db-stage usr/pwd postgres/postgres

Create it in the VPC above and create a new security group with full visibility (pg-anywhere-access). Pick configuration included in free tier. Enable Insights 
The endpoint name will go as the DB servername
[e.g. weather-db-stage.ctxox11y27cf.us-east-1.rds.amazonaws.com]
Set “Publicly Available” = True! Otherwise you can’t connect. 

IF the DB is NOT publicly available, than you need to use a jump server which is publicly accessible in the public subnet to which we can ssh into locally and we can jump from there in any private resource hosted in a private subnet
create a new user against the postgres instance of the DB

    CREATE USER weather_stage WITH password 'changeme!';

Create databases against the cloud instance:

    CREATE database cloud_weather_report;

(similar for all)

{idempotent because it won’t matter the state of the DB it will behave the same every time}
Create scripts to bring the DB structure in the cloud instance
    dotnet ef migrations script --idempotent -o 000_inittempdb.sql

execute scripts for all microservs to generate the DB structure in cloud
grant all privs for the newly created user for all instances


    GRANT ALL PRIVILEGES ON DATABASE cloud_weather_report to weather_stage;
    GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO weather_stage;
(similar for all)
In a real world you’ll have different users with different privs


- go back to task definition for weather_precipitation. 

create new env variables:

    CONNECTIONSTRINGS__APPDB  = Host=weather-db-stage.ctxox11y27cf.us-east-1.rds.amazonaws.com;Port=5432;Username=weather_stage;Password=changeme!;Database=cloud_weather_precipitation (e.g. Note - your db and domain will be different)


    ASPNETCORE_URLS  =  http://+:5000  (yes, only one underscore )

add the container.
create the task.
If changes are needed to the task, than you create a new revision(!) - zero downtime

do the same for weather-temperature

Now, because we have different appsettings for Report, we need to find out the port where the services are running - for this we need to run the services.

Go To Clusters and create a new service: “precipitation-service”, one task
pick the right VPC, 2 PRIVATE subnets, as for security we need to make sure that port 5000 is available. 
Edit selected security group → add rule for Custom TCP for port 5000 from anywhere.
Disable Auto-Assign public IP 

!!! Here you can set up “Service Discovery” !!! (not now)

Precipitation service is now running using the task definition above. Check logs! 
Now you can see that all is running OK and in the details tab of the task you’ll find the Private IP Address : 10.0.146.208

same for temperature

Complete the settings for the weather-report task than create a corresponding service, similar but it will run on a public subnet and will get a public IP address.

Finally we add a task for the data-loader to add data to the database
Since this is a manually triggered job, we need to run manually the task: “Run Task”
Run it with the default security in a private subnet and no automatic IP allocation

Normally at this point you can check that data is in the database in temperature and precipitation tables
