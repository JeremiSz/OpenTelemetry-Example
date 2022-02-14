docker run -d --name es01 --network=my-net \
-p 9200:9200 -p 9300:9300 -e discovery.type=single-node \
-e xpack.security.enabled=false -e reindex.ssl.verification_mode \
-e node.name=mynode \
docker.elastic.co/elasticsearch/elasticsearch:7.0.0