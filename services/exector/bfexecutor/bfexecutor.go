package bfexecutor

/*
#cgo CFLAGS: -O0 -fno-stack-protector
#cgo LDFLAGS: ${SRCDIR}/lib/bfexecutor.a -lm
#include <bfexecutor.h>
 */
import "C"
import (
	"errors"
	"fmt"
	"unsafe"
)

const MaxOutputLength = 1024

func RunBfCode(code, input string, maxOperations uint) (string, error) {
	var bcode = []byte(code)
	codePtr := (*C.char)(unsafe.Pointer(&bcode[0]))
	codeLen := C.int(len(bcode))

	var binput = []byte(input)
	inputPtr := (*C.char)(unsafe.Pointer(&binput[0]))
	inputLen := C.int(len(binput))

	var boutput [MaxOutputLength]byte
	outputPtr := (*C.char)(unsafe.Pointer(&boutput[0]))

	maxOutLen := C.int(MaxOutputLength)

	cmaxOperations := C.uint(maxOperations)

	var writtenBytes uint
	writtenBytesPtr := (*C.uint)(unsafe.Pointer(&writtenBytes))

	retCode := C.run_bf_code(codePtr, codeLen, inputPtr, inputLen, outputPtr, maxOutLen, cmaxOperations, writtenBytesPtr)

	if retCode != 0 {
		return "", errors.New(fmt.Sprintf("Error at code (%v)", retCode))
	}
	writtenBytes = uint(len(boutput))
	return string(boutput[:]), nil
}

func _main() {
	// 92 += 120
	// 93 += 1
	source := ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++>+"
	stdin := "1"
	stdout, err := RunBfCode(source, stdin, 500000)
	if err != nil {
		panic(err)
	}
	fmt.Println(stdout)
}
