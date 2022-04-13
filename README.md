<p align="center">
  <img src="https://i.imgur.com/V9zTL16.png" alt="Lazer image"/>
  <h2 style="text-align: center;">Shiroi - a new osu!lazer server</h2>
</p>

## Development
For development, you need development database & cache.
You can setup in on host, but i'm recommend it setup in docker.
Here some commands to setup development environment.
```
$ docker run -p 5432:5432 -e POSTGRES_USER=postgres -e POSTGRES_DB=lazer -e POSTGRES_PASSWORD=123321 -d --name lazer-db postgres:14
$ docker run -p 6379:6379 --name lazer-cache -d redis:7.0-rc3
```