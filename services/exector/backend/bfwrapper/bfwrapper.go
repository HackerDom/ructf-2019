package bfwrapper

import (
	"bytes"
	"errors"
	"io/ioutil"
	"os/exec"
)

func RunBfCode(code string, input []byte, maxOperations uint) ([]byte, error) {
	cmd := exec.Command("bfwrapper/bfexecutor/bfexecutor", code)
	stdout := &bytes.Buffer{}
	stdin := &bytes.Buffer{}
	stderr := &bytes.Buffer{}

	if _, err := stdin.Write(input); err != nil {
		panic(err)
	}

	cmd.Stdout = stdout
	cmd.Stdin = stdin
	cmd.Stderr= stderr

	err := cmd.Run()

	if err != nil {
		if _, ok := err.(*exec.ExitError); !ok {
			panic(err)
		}

		if retErr, err := ioutil.ReadAll(stderr); err != nil {
			return nil, err
		} else {
			return nil, errors.New(string(retErr))
		}
	}
	data, err := ioutil.ReadAll(stdout)
	if err != nil {
		return nil, err
	}
	return data, nil
}
