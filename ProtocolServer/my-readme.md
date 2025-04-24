# DATE: 23-April-2025 
# SOURCE: https://github.com/aykutalparslan/high-perfomance-tcp-server
# RESULT: WORKED FINE, LOCALLY AND ON AZURE CONTAINER APPS

# Debug
## IF .SocketException (48): Address already in use: KILL the process
sudo lsof -i :<PORT>
sudo kill -9 <PID>


# Docker Commands
docker build --platform linux/x86_64 -t protocolserver -f Dockerfile .

docker container run  --name protocolserver -it -p 11001:11001 protocolserver



# Azure Container Registry
- Create 
- Access Keys: enable Admin user, QZwnwTg1KIUKA/GWflC3eNQo5jz2Y5dFmpeJtTO5Ig+ACRBBgq5P

acr=plxccoepnraca
app=protocolserver
tag=v1
docker tag $app $acr.azurecr.io/$app:$tag

az login --tenant  pluxeegroup.onmicrosoft.com
az acr login -n $acr
docker push $acr.azurecr.io/$app:$tag
az acr repository list -n $acr