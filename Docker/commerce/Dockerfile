# escape=`
FROM avivasolutionsnl.azurecr.io/sitecore-xc-commerce:9.1.0-20190528

SHELL ["powershell", "-NoProfile", "-Command", "$ErrorActionPreference = 'Stop';"]

# Install VS 2017 remote debugging
RUN choco install -y --params="Quiet" visualstudio2017-remotetools

ADD Boot.ps1 C:\