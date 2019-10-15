# Eleia

[![Build Status](https://dev.azure.com/ktos/Eleia/_apis/build/status/ktos.Eleia?branchName=master)](https://dev.azure.com/ktos/Eleia/_build/latest?definitionId=5&branchName=master)
[![Code Coverage](https://img.shields.io/azure-devops/coverage/ktos/Eleia/5)](https://dev.azure.com/ktos/Eleia/_build?definitionId=5)
[![Tests](https://img.shields.io/azure-devops/tests/ktos/eleia/5)](https://dev.azure.com/ktos/Eleia/_build?definitionId=5)
[![License](https://img.shields.io/github/license/ktos/Eleia)](https://github.com/ktos/Eleia/blob/master/LICENSE)

The machine-learning powered bot for detecting, and commenting about unmarked
code on 4programmers.net forum.

[Polska wersja README](https://github.com/ktos/Eleia/blob/master/README.pl.md)

## Short description

Eleia is tracking new posts on the 4programmers.net forum, and is analyzing
every post by removing all HTML tags (including `<code>`), then splitting it
into paragraphs and then performing a classification between "code" and "text"
for every paragraph.

If any of the paragraphs is classified as "code", then it is possible to post
comment to this post with a text "Hey! Improve your post by adding ``` tags!"
or something.

By default, Eleia will go to sleep and return after specified amount of time
to perform same operations over and over again. Posts which were classified
one time won't be analyzed again, but this state is in-memory only, so will
be removed after application restart.

## Usage

Eleia must have a configuration before you can use it. You can use configuration
file `appconfig.json`, environment variables starting with `ELEIA_` prefix or
use commandline arguments.

For example, to start Eleia only to watch and analyze new posts, on the dev
server, you can just simply run:

```batch
eleia
```

While to run on real 4programmers.net:

```batch
eleia --useDebug4p false
```

To post comments automatically, you need to provide username and password:

```batch
eleia --username "Some User" --password "Password" --useDebug4p false --postComments true
```

All possible configuration options are:

* `username` -> username to log in with Coyote,
* `password` -> password to authenticate with Coyote,
* `useDebug4p` -> should use `dev.4programmers.info` or `4programmers.net`?,
* `postComments` -> should post comments when unformatted code is found?,
* `timeBetweenUpdates` -> what is the time sleeping before getting new batch of posts?,
* `treshold` -> what is the treshold of "code" classification triggering posting a comment (by default: 0.99).

## Machine Learning

It is using ML.NET framework to perform machine learning-based classification,
and AutoML was used to generate model.

[basictrainingdata.tsv](https://github.com/ktos/Eleia/blob/master/basictrainingdata.tsv)
was a file which the training was performed on.

## Contributing

You are welcome to contribute to Eleia!

Please start with creating a new issue, so we can discuss what are you trying
to achieve. Then fork this repo to your own profile, fix bugs or add new things
and send me a pull request.

Every PR must be built and tested on Azure Pipelines, it is done automatically.
