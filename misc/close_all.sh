docker stop $(docker ps -aq)
docker rm $(docker ps -aq)
docker network rm my-net
#docker rmi $(docker images -q)
#docker volume rm $(docker volume ls -q)