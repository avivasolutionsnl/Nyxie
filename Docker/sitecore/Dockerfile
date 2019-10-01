# escape=`
ARG BASE_IMAGE
FROM ${BASE_IMAGE}

SHELL ["powershell", "-NoProfile", "-Command", "$ErrorActionPreference = 'Stop';"]

RUN choco install -y --params="Quiet" visualstudio2017-remotetools

COPY Boot.ps1 C:/