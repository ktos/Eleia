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

## Usage (CLI)

Since version 0.6, Eleia supports a set of commandline parameters to be run
with, where every is used in a format of `--parameter <value>` or `-p <value>`.

* `-u` or `--username` -> username to log in with to Coyote. Default is null,
and may be null only if `-c` is not `true`.
* `-p` or `--password` -> password to use in log in,
* `-t` or `--timeBetweenUpdates` -> time (in minutes) to wait before getting
another set of posts, if `-s` was not used. If given 0, bot will run only once
and then finishes the job, like `-r`. Default is 60.
* `-n` or `--nagMessage` -> message to be posted in comment to post, you can use
`{0}` in which probability calculated by the model will be put into,
* `-d` or `--useDebug4p` -> `true` or `false`. If `true`, `dev.4programmers.info`'s
endpoints will be used instead of `4programmers.net`. Default is `true`.
* `-c` or `--postComments` -> `true` or `false`. If `true`, after finding problem
with post, a comment with nag message will be posted to that post. Default is
`false`.
* `-r` or `--runOnce` -> `true` or `false`. If `true`, the application will run
only one time, if not - it will go to sleep for `-t` minutes to perform another
analysis. Default is `false`.
* `-s` or `--runOnSet` -> starts a single run (like `-r`) but on provided list of
post ids, separated by commas, instead of getting new posts. You may use it like
`-s 1,2,3,4` to analyze posts of id values equal to 1, 2, 3 and 4. Implies `-r`.
* `--blacklist` -> comma separated list of forum ids, from which posts will be
ignored,
* `--ignoreAlreadyAnalyzed` -> ignores "already analyzed" database, analyzes
every post from not blacklisted forums, does not create "already analyzed"
database on exit,
* `--help` -> displays the help screen and exits,
* `--version` -> displays the version information and exists.

### Examples

```bash
# will run only one time, analyze the posts, post comments if threshold is met
# and exit - on dev version of 4programmers.info
eleia --u someuser -p someSECRETpassw0rd --useDebug4p true --postComments true --runOnce true

# will run continously analysing posts, sleeping for 15 minutes between getting
# new set to analyze, won't post comments, will use real 4programmers.net
eleia -d false -c false -t 15

# will analyze only posts of id 14122 and 221
eleia --runOnSet 221,14122
```

## Configuration via config file and environment variables

Apart from CLI parameters, it's possible to configure application using
environment variables or configuration file to provide username, password
and similar. This was considered mainly for Docker images.

**Warning:** Configuration has a priority over CLI! For example if you set
the username both in config file and `-u` parameter, value from the config file
will be used.

Possible configuration options are:

* `username` -> (string) username to log in with Coyote,
* `password` -> (string) password to authenticate with Coyote,
* `useDebug4p` -> (bool) should use `dev.4programmers.info` or `4programmers.net`?,
* `postComments` -> (bool) should post comments when unformatted code is found?,
* `timeBetweenUpdates` -> (int) what is the time sleeping before getting new batch of posts?,
* `threshold` -> (float) what is the threshold of "code" classification triggering posting a comment (by default: 0.99),
* `nagMessage` -> (string) what is the nag message posted as comment? `{0}` will be replaced with probability of unformatted code,
* `blacklist` -> (string) comma-separated list of forum ids from which posts will be ignored.

All these options may be used in a `appsettings.json` file in the current directory.
Apart from that, you may set verbosity level, by setting `Logging:LogLevel:Default`
configuration option (possible values: `Debug`, `Information`, `Warning` and
`Error`).

Example configuration file:

```json
{
  "username": "some user",
  "password": "verySECRETpassw0rd!",
  "useDebug4p": true,
  "postComments": false,
  "timeBetweenUpdates": 30,
  "threshold": 0.95,
  "nagMessage": "Hey! Something is wrong with code in your post!",
  "Logging": {
    "IncludeScopes": false,
    "LogLevel": {
      "Default": "Debug"
    }
  }
}
```

If you prefer to use environment variables, they must start with `ELEIA_` prefix,
e.g.:

```bash
export ELEIA_username=someuser2
export ELEIA_useDebug4p=false

# if you want to set verbosity level, you have to use __ as section separator
export ELEIA_Logging__LogLevel__Default=Error
```

## Machine Learning

It is using ML.NET framework to perform machine learning-based binary classification,
and AutoML (`mlnet auto-train`) was used to generate model.

Exact run:

```batch
mlnet auto-train -T binary-classification -d trainingdata2.tsv -o ML --label-column-name code -x 180
```

[trainingdata2.tsv](https://github.com/ktos/Eleia/blob/master/trainingdata2.tsv)
was a file which the training was performed on.

[trainingdata2.log](https://github.com/ktos/Eleia/blob/master/trainingdata2.log)
is a log of auto-training, SdcaLogisticRegressionBinary was decided with
accuracy of 0.9572.

I was also running longer training sessions than 180 seconds, but still that
algorithm was decided as the best, longer time had no visible impact.

## Contributing

You are welcome to contribute to Eleia!

Please start with creating a new issue, so we can discuss what are you trying
to achieve. Then fork this repo to your own profile, fix bugs or add new things
and send me a pull request.

Every PR must be built and tested on Azure Pipelines, it is done automatically.

Published versions are built on Azure Pipelines on every new tag, I am using
SemVer to versioning releases.
