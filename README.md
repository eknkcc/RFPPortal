# RFPPortal
Proposal Request Portal is the platform which RFPs are posted by the authenticated third party (DevxDao).Public users can also bid.

- The project was developed with dotnet core 3.1 in C# as language.
- jQuery UI - v1.10.4 and Razor was used to develop the frontend.

## Test Environment
Address: 193.140.239.52:1098 <br>
Test Admin User: <br>
username: Ekin <br>
password: 1parola1 <br>

## Setup
### Build and run with docker-compose
Docker must be installed on your system.

Enter your smtp information to `\PathToSolution\RFPPortal\RFPPortalWebsite\appsettings.json` under `SMTP` section.

Under the project folder, type
```
docker-compose up --build
```
Docker-compose downloads/creates(if necessary) builds and runs;
- Mysql: 5.7 instance image
- rfpportalwebsite image

when all containers are up, you can access the application from
`https://localhost:6443`
and the mysql database is accessible on localhost:3317.

The container endpoint ports can be changed from docker-compose.override.yml file.

After proper setup, the mysql instance should have a database named 'daorfpdb'.
The daorfpdb database should have tables below;
- Users
- UserLogs
- Rfps
- RfpBids
- ErrorLogs
- ApplicationLogs
- __EFMigrationHistory

## Usage
Your smtp information must be entered to SMPT section in the appsettings.json file.
This is required to create a user because the registration process is completed with the activation mail.

### Creating a Public User
Simply Click the login button in the upper right corner.
A model will appear with a 'Sign Up' tab.
Fill out the form and hit the sign up button. An activation email will be sent to you. Click on the link specified in the activation e-mail. You will be redirected to the application url using your default browser. Once the page appeared, you should see a toaster indicating that the activation was successful.<br>
As a public user

### Creating an Internal User
Signing up with an internal email automatically creates an internal user.<br>
Internal accounts are recognized by RFPPortal via an api provided by DEVxDAO.

### Creating an Admin User
To create an admin user, first a public user should be created and user type should be updated to 'admin' manually from the database.
The database can be accessed by docker cli.
```shell
	docker exec -it rfpportal-rfpportal_db bash -l
	/# mysql -u root -p daorfpdb
	> update Users set UserType = 'Admin' where Username = 'username of the user that is required to be admin';
```
Where "rfpportal-rfpportal_db" is the name of the container. All running containers can be listed with `docker ps -a` command from the terminal.

### Creating RFP
When logged in to the application as an Admin user, there appears a 'New RFP' navigation button on the top left corner in addition to 'My Bids' and 'RFP List' navigation buttons.
Clicking it redirects to an RFP form page including the fields below;
- Type of the Job
- Job Description
- Total Price for the Job
- Job Start Date
- Job Completion Date

Filling the form and submitting it creates an internal RFP which only the internal users are able to see and bid on.<br>
The posted RFP is added to the list of RFPs shown in the main page.<br>

#### RFP Card
RFP card shows;
- Job Type
- Posted date
- Job Description
- Time frame
- Type of the RFP (public/internal)
- Days remaining to type/status change (internal/public/expired)
- Status of the RFP (completed/continues/expired)

VA users are expected to submit bids to rfps that are in internal state and the winning bid to be determined within 15 days.<br>
If a winner still has not been determined at the end of the 15th day, the RFP becomes public. Now public users can view and bid the RFP on their page.

### Creating, Editing and Deleteing a bid
#### Creating
Clicking on the RFP card that you want to bid, redirects you to RFP Detail page. RFP Detail page shows all RFP information and all the bids as a list.<br>
If bidding continues, there is a `New Bid` button on the top-right corner. Clicking it brings a modal bidding form including fields below;
- Name and Surname (autofilled)
- Username (autofilled)
- Email (autofilled)
- Timeframe
- Amount
- Additional Notes

You can enter proposed amount, timeframe and additional notes (optional) and submit it by clicking the `Submit Bid` button placed on the bottom-right of the modal.
After proper submitting, there toasts a green `Success` message on the top-right corner of the page and the submitted bid is added on the bid list below the RFP information pane.

#### Editing and Deleting
In the RFP Details page, if the user has an active bid, the row in the list below the RFP information pane, has 'Edit' and 'Delete' buttons which allows the user delete and edit their active bids.

### Choosing the Winning Bid
Only an admin type of user is able to choose the winning bid.<br>
Admin users' RFP detail page has a star button placed on the end of the every bid row. Clicking it brings a confirm pop-up. After confirming, there appears a green 'Success' toaster on the top-right corner of the page and the star in the selected bid row becomes yellow.<br>
Choosing the winning bid changes the status of the RFP from 'bidding continues' to 'bidding ended'.

### My Bids Page
Clicking on the 'My Bids' navigator button at the top-left corner of the home page, every user is redirected to the 'My Bids' page.<br>
The users can see their bidding history and RFP details of the belonging bid.

## Entities
###
- [ApplicationLog](docs/Entity/ApplicationLog.md)
- [ErrorLog](docs/Entity/ErrorLog.md)
- [Rfp](docs/Entity/Rfp.md)
- [RfpBid](docs/Entity/RfpBid.md)
- [User](docs/Entity/User.md)
- [UserLog](docs/Entity/UserLog.md)

## Testing
A mysql database should be up and running with a testing environment setup.<br>
The easiest and recommended way is pulling a mysql docker image and run in a docker container with minimum parameters.
```shell
docker run --detach --name=test-mysql -p 3317:3306  --env="MYSQL_ROOT_PASSWORD=mypassword" mysql
```
To access the mysql instance in the container:
```shell
docker exec -it test-mysql bash -l
```
To access the database from the mysql container terminal :
```shell
mysql -u root -p
Enter Password: **********
mysql>
```

The root password, the expose port and many other parameters can be changed optionally.
The test database connection string should be written under the PlatformSettings section taking place in the ``` \PathToSolution\RFPPortalWebsite\appsettings.json ``` file.<br>
Example:
```json
"PlatformSettings": {
    "DbConnectionString": "Server=localhost;Port=3317;Database=test_daorfpdb;Uid=root;Pwd=mypassword;",
    "InternalBiddingDays": 21,
    "PublicBiddingDays": 21
}
```

After configuring the database, run the following commands from the project directory.

```shell
dotnet test --filter DisplayName~Authorization_Tests
dotnet test --filter DisplayName~BidController_Tests
dotnet test --filter DisplayName~RfpController_Tests
```

Code documentation files in <XML> format is autogenerated everytime the project is build under bin folder.

## HTTPS Configuration	
To enable HTTPS, https settings of the application should be added to the Docker Container configuration and the an ssl certificate file should be introduced if necessary. <br>

Example of enabling HTTPS using 'docker-compose.override.yml' file:

```yml
rfpportalwebsite:
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=https://+;http://+:80
      - ASPNETCORE_HTTPS_PORT=443
```
`https://+` is added to ASPNETCORE_URLS and 443 port is defined as https port with adding `ASPNETCORE_HTTPS_PORT=443`<br>

An ssl certificate can be generated and placed in a location on the machine where the docker container is running.<br>
One way to generate an SSL certificate is explained [here](https://docs.microsoft.com/en-us/dotnet/core/additional-tools/self-signed-certificates-guide).

The presentation of the generated ssl certificate in the docker compose file is as follows:
```yml
rfpportalwebsite:
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=https://+;http://+:80
      - ASPNETCORE_HTTPS_PORT=443
# Password for the certificate
      - ASPNETCORE_Kestrel__Certificates__Default__Password=< password of the generated certificate >
# Path of the certificate file
      - ASPNETCORE_Kestrel__Certificates__Default__Path= < location of the ssl certificate in docker container. Example: '/https/aspnetapp.pfx' > 
    volumes:
	# Mount the local volume where the certificate exists to docker container
      - < location of the ssl certificate in the host machine> : < location of the ssl certificate in docker container. Example: '~/.aspnet/https:/https:ro'>	
```