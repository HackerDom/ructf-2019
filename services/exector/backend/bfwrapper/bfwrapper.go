package bfwrapper

import (
	"bytes"
	"errors"
	"fmt"
	"io/ioutil"
	"os"
	"os/exec"
)

type BfExecutor struct {
	BinPath string
}

func (bfExecutor *BfExecutor) RunBfCode(code string, input []byte, maxOperations uint) ([]byte, error) {
	cmd := exec.Command(bfExecutor.BinPath, code)
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

func (bfExecutor *BfExecutor) Init(binPath string) error {
	fi, err := os.Stat(binPath)
	if err != nil {
		return errors.New(fmt.Sprintf("can not init executor: %v", err))
	}
	if fi.IsDir() {
		return errors.New("bin path is a directory")
	}
	bfExecutor.BinPath = binPath
	return nil
}
