package main

import (
	"encoding/json"
	"errors"
	"io/ioutil"
)

type Config struct {
	ServerHost string
	ServerPort uint
	BackendPort uint
	StaticDir string
}

func ParseConfig(path string) (*Config, error) {
	data, err := ioutil.ReadFile(path)
	if err != nil {
		return nil, errors.New("can not read config file: " + err.Error())
	}
	var config Config
	if err := json.Unmarshal(data, &config); err != nil {
		return nil, errors.New("can not parse config file: " + err.Error())
	}
	return &config, nil
}
