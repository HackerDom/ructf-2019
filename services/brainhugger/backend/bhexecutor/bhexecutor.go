package bhexecutor

import "C"
import (
	"bytes"
	"errors"
	"fmt"
	"io/ioutil"
	"os"
	"os/exec"
)

type BhExecutor struct {
	BinPath string
}

func (bhExecutor *BhExecutor) RunBhCode(code string, input []byte, maxOperations uint) ([]byte, error) {
	cmd := exec.Command(bhExecutor.BinPath, code)
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

func (bhExecutor *BhExecutor) Init(binPath string) error {
	fi, err := os.Stat(binPath)
	if err != nil {
		return errors.New(fmt.Sprintf("can not init executor: %v", err))
	}
	if fi.IsDir() {
		return errors.New("bin path is a directory")
	}
	bhExecutor.BinPath = binPath
	return nil
}
