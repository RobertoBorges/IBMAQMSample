# IBMAQMSample

## Running with the default configuration
You can run a queue manager with the default configuration and a listener on port 1414 using the following command.  For example, the following command creates and starts a queue manager called `QM1`, and maps port 1414 on the host to the MQ listener on port 1414 inside the container, as well as port 9443 on the host to the web console on port 9443 inside the container:

```sh
docker run \
  --env LICENSE=accept \
  --env MQ_QMGR_NAME=QM1 \
  --env MQ_APP_PASSWORD=passw0rd \
  --publish 1414:1414 \
  --publish 9443:9443 \
  --detach \
  icr.io/ibm-messaging/mq
```

# Default developer configuration

If you build this image with MQ Advanced for Developers, then an optional set of configuration can be applied automatically.  This configures your Queue Manager with a set of default objects that you can use to quickly get started developing with IBM MQ. If you do not want the default objects to be created you can set the `MQ_DEV` environment variable to `false`.

## Environment variables

The MQ Developer Defaults supports some customization options, these are all controlled using environment variables:

* **MQ_DEV** - Set this to `false` to stop the default objects being created.
* **MQ_ADMIN_PASSWORD** - Changes the password of the `admin` user, default "passw0rd". Must be at least 8 characters long.
* **MQ_APP_PASSWORD** - `DEV.APP.SVRCONN` channel is secured and only allow connections that supply a valid userid "app" and password "passw0rd". Must be at least 8 characters long, and you should change for your implementation.

## Details of the default configuration

The following users are created:

* User **admin** for administration.  Default password is **passw0rd**.
* User **app** for messaging (in a group called `mqclient`).  No password by default.

Users in `mqclient` group have been given access connect to all queues and topics starting with `DEV.**` and have `put`, `get`, `pub`, `sub`, `browse` and `inq` permissions.

The following queues and topics are created:

* DEV.QUEUE.1
* DEV.QUEUE.2
* DEV.QUEUE.3
* DEV.DEAD.LETTER.QUEUE - configured as the Queue Manager's Dead Letter Queue.
* DEV.BASE.TOPIC - uses a topic string of `dev/`.

Two channels are created, one for administration, the other for normal messaging:

* DEV.ADMIN.SVRCONN - configured to only allow the `admin` user to connect into it.  A user and password must be supplied.
* DEV.APP.SVRCONN - does not allow administrative users to connect.  Password is optional unless you choose a password for app users.

## Web Console

By default the MQ Advanced for Developers image will start the IBM MQ Web Console that allows you to administer your Queue Manager running on your container. When the web console has been started, you can access it by opening a web browser and navigating to https://<Container IP>:9443/ibmmq/console. Where <Container IP> is replaced by the IP address of your running container.

When you navigate to this page you may be presented with a security exception warning. This happens because, by default, the web console creates a self-signed certificate to use for the HTTPS operations. This certificate is not trusted by your browser and has an incorrect distinguished name.

If you choose to accept the security warning, you will be presented with the login menu for the IBM MQ Web Console. The default login for the console is:

* **User:** admin
* **Password:** passw0rd

If you wish to change the password for the admin user, this can be done using the `MQ_ADMIN_PASSWORD` environment variable.

If you do not wish the web console to run, you can disable it by setting the environment variable `MQ_ENABLE_EMBEDDED_WEB_SERVER` to `false`.
