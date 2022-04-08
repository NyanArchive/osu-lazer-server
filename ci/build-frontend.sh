# clone repository...
git clone https://git.kotworks.cyou/DHCPCP/lazer-frontend.git

cd lazer-frontend

export CI=true
# install modules
npm install

#build
npm run build

mv build ..
