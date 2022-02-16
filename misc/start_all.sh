docker network create -d bridge my-net

#elastic search
docker run -d -v ./database --name es01 --network=my-net -p 9200:9200 -p 9300:9300 \
-e discovery.type=single-node -e xpack.security.enabled=false -e reindex.ssl.verification_mode -e node.name=mynode \
docker.elastic.co/elasticsearch/elasticsearch:7.0.0

#Prometheus & Collector
docker compose -f ./support/docker-compose.yaml up &

#mongo db
docker run -v ./database --network=my-net --name mongo -d -p 27017:27017 mongo:5.0

#rabbit
docker run -d --hostname my-rabbit \
--network=my-net \
-p 15691:15691 -p 15692:15692 \
-p 25672:25672 \
-p 4369:4369 \
-p 5671:5671 -p 5672:5672 \
--name rabbit rabbitmq:3

#postgresql
docker run --name some-postgres \
-p 5432:5432 \
-e POSTGRES_PASSWORD=mysecretpassword \
-v newData -d postgres

#jaeger
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
-p 9411:9411  \
-e SPAN_STORAGE_TYPE=elasticsearch \
-e ES_SERVER_URLS=http://es01:9200 \
jaegertracing/all-in-one:1.30