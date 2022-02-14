docker run --network=my-net --name some-postgres -p 5432:5432 \
-e POSTGRES_PASSWORD=mysecretpassword \
-v newData -d postgres