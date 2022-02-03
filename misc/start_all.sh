docker network create -d bridge my-net

docker run -d --network=my-net --name jaeger \
-e COLLECTOR_ZIPKIN_HOST_PORT=:9411 \
-p 5775:5775/udp \
-p 6831:6831/udp \
-p 6832:6832/udp \
-p 5778:5778 \
-p 16686:16686 \
-p 14250:14250 \
-p 14268:14268 \
-p 14269:14269 \
-p 9411:9411 jaegertracing/all-in-one:1.30

docker compose -f ./open_collector_support/docker-compose.yaml up &

docker run -v "C:\Extra\opentelemetry\misc\database" --network=my-net --name mongo -d -p 27017:27017 mongo:5.0
#docker exec mongo ./mongo_support/set_up_db.sh

docker run -d --hostname my-rabbit --network=my-net -p 15691:15691 -p 15692:15692 -p 25672:25672 -p 4369:4369 -p 5671:5671 -p 5672:5672 --name rabbit rabbitmq:3 