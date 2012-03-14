# runtime #
.text
_print:
	li $v0, 4
	syscall
	jr $ra

_printi:
	li $v0, 1
	syscall
	jr $ra

_flush:
	jr $ra

_getchar:
	li $v0, 9 
	li $a0, 2
	syscall
	move $a0, $v0
	li $a1, 2
	li $v0, 8
	syscall
	move $v0, $a0
	jr $ra

_ord:
	lb $a1, ($a0)
	li $v0, -1
	beqz $a1, _ord_1
	lb $v0, ($a0)
_ord_1:
	jr $ra

_chr:
	move $a1, $a0
	li $v0, 9
	li $a0, 2
	syscall
	sb $a1, ($v0)
	sb $zero, 1($v0)
	jr $ra

_size:
	move $v0, $zero
_size_1:
	lb $a1, ($a0)
	beq $a1, $zero, _size_2
	add $v0, $v0, 1
	add $a0, $a0, 1
	j _size_1
_size_2:
	jr $ra

_substring:
	add $a1, $a0, $a1
	move $a3, $a1
	li $v0, 9
	add $a2, $a2, 1
	move $a0, $a2
	add $a0, $a0, 1 
	syscall
	add $a2, $a2, $a3
	add $a2, $a2, -1
	move $a0, $v0
_substring_1:
	beq $a1, $a2, _substring_2
	lb $a3, ($a1)
	sb $a3, ($a0)
	add $a1, $a1, 1
	add $a0, $a0, 1 
	j _substring_1
_substring_2:
	sb $zero, ($a0)
	jr $ra

_concat:
	sw $a0, -4($sp)
	sw $a1, -8($sp)
	sw $ra, -12($sp)
	jal _size
	li $a3, 1
	add $a3, $a3, $v0
	lw $a0, -8($sp)
	jal _size
	add $a3, $a3, $v0
	move $a0, $a3
	li $v0, 9
	syscall 
	move $a3, $v0
	move $a0, $v0
	lw $a1, -4($sp)
	jal _copy
	move $a0, $v0
	lw $a1, -8($sp)
	jal _copy
	move $v0, $a3
	lw $ra, -12($sp)
	jr $ra

_not:
	seq $v0, $a0, 0
	jr $ra

_exit:
	li $v0, 10
	syscall

_strcmp:
_strcmp_1:
	lb $a2, ($a0)
	lb $a3, ($a1)
	beq $a2, $zero, _strcmp_4
	beq $a3, $zero, _strcmp_4
	bgt $a2, $a3, _strcmp_2
	blt $a2, $a3, _strcmp_3
	add $a0, $a0, 1
	add $a1, $a1, 1
	j _strcmp_1
_strcmp_2:
	li $v0, 1
	jr $ra
_strcmp_3:
	li $v0, -1
	jr $ra
_strcmp_4:
	bne $a2, $zero, _strcmp_2
	bne $a3, $zero, _strcmp_3
	li $v0, 0
	jr $ra

_record:
	li $v0, 9
	syscall
	move $v1, $v0
	add $a0, $a0, $v0
_record_1:
	sw $zero, ($v1)
	add $v1, $v1, 4
	bne $v1, $a0, _record_1
	jr $ra

_array:
	sll $a0, $a0, 2
	li $v0, 9
	syscall
	move $v1, $v0
	add $a0, $a0, $v0
_array_1:
	sw $a1, ($v1)
	add $v1, $v1, 4
	bne $v1, $a0, _array_1
	jr $ra

_malloc:
	li $v0,9
	syscall
	jr $ra

_copy:
_copy_1:
	lb $a2, ($a1)
	beq $zero, $a2 _copy_2
	sb $a2, ($a0)   
	add $a0, $a0, 1
	add $a1, $a1, 1
	j _copy_1
_copy_2:
	sb $zero, ($a0)
	move $v0, $a0
	jr $ra