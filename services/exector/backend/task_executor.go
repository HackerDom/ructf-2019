package main

/*
#include<unistd.h>
void suicide() {
	*(int*)0 = 0;
}
*/
import "C"
import (
	"bytes"
	"encoding/json"
	"errors"
	"exector/backend/bhwrapper"
	"exector/backend/storage"
	"fmt"
	"log"
)

type TaskExecutor struct {
	CounterFilename string
	TasksStorage    storage.Storage
	BhExecutor      bhwrapper.BhExecutor
}

const (
	DONE = 0
	ERROR = 2
)

var Status2String = map[int]string {
	0: "DONE",
	1: "RUNNING",
	2: "ERROR",
}

type TaskResult struct {
	Status int
	Stdout string
	Error string
}

func (tr TaskResult) String() string {
	return fmt.Sprintf(
		"TaskResult(status=%v, stdout='%v', error='%v')",
		Status2String[tr.Status],
		tr.Stdout,
		tr.Error,
		)
}

func jsonMarshalWithoutEscaping(t interface{}) ([]byte, error) {
	buffer := &bytes.Buffer{}
	encoder := json.NewEncoder(buffer)
	encoder.SetEscapeHTML(false)
	err := encoder.Encode(t)
	return buffer.Bytes(), err
}

func (exec *TaskExecutor) SaveTask(result TaskResult, taskId uint) {
	rawTaskResult, err := jsonMarshalWithoutEscaping(result)
	if err != nil {
		log.Printf("Can not save task due to error: %v\n", err)
		return
	}
	err = exec.TasksStorage.Set(taskId, rawTaskResult)
	if err != nil {
		log.Printf("Can not save task due to error: %v\n", err)
		return
	}
}

func (exec *TaskExecutor) ExecuteTask(source string, stdin []byte) ([]byte, error) {
	result, err := exec.BhExecutor.RunBhCode(source, stdin, 50000)
	return result, err
}

func (exec *TaskExecutor) ProcessTask(taskId uint, source string, stdin []byte) {
	exec.SaveTask(TaskResult{1, "", ""}, taskId)
	stdout, err := exec.ExecuteTask(source, stdin)
	var result TaskResult
	if err != nil {
		result = TaskResult{ERROR, "", fmt.Sprint(err)}
	} else {
		result = TaskResult{DONE, string(stdout), ""}
	}
	exec.SaveTask(result, taskId)
}

func (exec *TaskExecutor) AddTask(source string, stdin []byte) uint {
	taskId := exec.TasksStorage.GetTaskCount()
	go exec.ProcessTask(taskId, source, stdin)
	exec.TasksStorage.IncTaskCount()
	return taskId
}

func (exec *TaskExecutor) TaskInfo(taskId uint) ([]byte, error) {
	if taskId >= exec.TasksStorage.GetTaskCount() {
		return nil, errors.New("no such task")
	}
	value, err := exec.TasksStorage.Get(taskId)
	if err != nil {
		return []byte("{\"Status\": \"2\"}"), nil
	} else {
		return value, nil
	}
}

func (exec *TaskExecutor) Init(taskDir, counterFilename string, taskBlockSize uint, bhExecutorBinPath string) error {
	exec.CounterFilename = counterFilename
	exec.TasksStorage.Init(taskDir, counterFilename, 40000)
	var bhExecutor bhwrapper.BhExecutor
	if err := bhExecutor.Init(bhExecutorBinPath); err != nil {
		return err
	}
	exec.BhExecutor = bhExecutor
	return nil
}
