# ChillChaser web API
## To run in prod
1. Download the release docker-compose.yml
2. Create file db_password.txt containing only the db password eg. `password`
3. Create another file db_passfile.txt containing the database login details in postgres passfile format eg. `db:5432:chillchaser_db:chillchaser_user:password`
4. Launch it using docker compose eg. `docker compose up` in the root folder of the project
## To run in dev
1. Install ef tool `dotnet tool install --global dotnet-ef`
2. Start the development docker compose environment `docker compose -f docker-compose.dev.yml up` (optonally use the `-d` flag at the end to run it in the background)
3. Wait for postgres to be ready
4. Run all migrations `dotnet ef database update --project ChillChaser`
5. Now the API can be started
6. When done, shutdown the docker compose container by running `docker compose -f docker-compose.dev.yml down` if run in detached mode or `ctrl+c` if in non detached mode.