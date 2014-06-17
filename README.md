## Reverse Proxy

_A simple ASP.NET reverse proxy website that anyone can use._

This project aims to create a very simple mechanism for serving
static files from a separate domain than the main website.

The Reverse Proxy site can then easily be deployed to CDN's or 
to Azure using the Traffic Manager to distribute the files globally.

There are several reasons why it's a good idea to serve static files 
from separate domains (or subdomains).

1. No cookies are sent or received making the payload smaller
2. Free up the browser's request limit to serve the pages faster
3. Distribute globally so the files has the shortest route to the users

### Install

Fork or copy this project and deploy it to your own web server. No 
server components or NuGet packages needed to run this site.

The reverse proxy mechanism is all located in a very small HttpModule. 
This module activates when a file is requested that doesn't exist. In 
that case, the module will download the file and serve it immediately 
to the user and then save it to disk. Any subsequent request will then 
hit the static file on disk and served super fast without ASP.NET is
involved at all.

Let's say you create a website at `foo.com` where you host this 
Reverse Proxy site.

You also have an existing website at `bar.com`. You can now send a request
to `foo.com` that looks like this: `http://foo.com/bar.com/images/logo.png`.

Notice that the domain `bar.com` is the first path segment in the URL to 
the Reverse Proxy website. That informs the Reverse Proxy how to resolve
the actual image located on `bar.com`.

As of recently, [MiniBlog](https://github.com/madskristensen/miniblog/) 
has native support for reverse proxy file serving and is fully compatible
with this Reverse Proxy.