#!/bin/bash

#sleep 10
cpufreq-set -u 240MHz
mono radio_app.exe &> /dev/null &
