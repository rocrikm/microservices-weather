#!/bin/bash
set -e

aws ecr get-login-password --region us-east-1 --profile weather-ecr-agent | docker login --username AWS --password-stdin 021596293162.dkr.ecr.us-east-1.amazonaws.com
docker build -f ./Dockerfile -t cloud-weather-data-loader:latest .
docker tag cloud-weather-data-loader:latest 021596293162.dkr.ecr.us-east-1.amazonaws.com/cloud-weather-data-loader:latest
docker push 021596293162.dkr.ecr.us-east-1.amazonaws.com/cloud-weather-data-loader:latest


