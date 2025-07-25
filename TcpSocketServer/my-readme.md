# DATE: 22-April-2025 
# RESULT: DIDN'T WORK! WORKED FINE LOCALLY BUT CONTAINER CRASHED ON AZURE CONTAINER APPS

# Debug
## IF .SocketException (48): Address already in use: KILL the process
sudo lsof -i :<PORT>
sudo kill -9 <PID>


# Docker Commands
docker build --platform linux/x86_64 -t tcpsocketserver -f Dockerfile .

docker container run  --name tcpsocketserver -it -p 11001:11001 tcpsocketserver



# Azure Container Registry
- Create 
- Access Keys: enable Admin user, QZwnwTg1KIUKA/GWflC3eNQo5jz2Y5dFmpeJtTO5Ig+ACRBBgq5P

acr=plxccoepnraca
app=tcpsocketserver
tag=v3
docker tag $app $acr.azurecr.io/$app:$tag

az login --tenant  pluxeegroup.onmicrosoft.com
az acr login -n $acr
docker push $acr.azurecr.io/$app:$tag