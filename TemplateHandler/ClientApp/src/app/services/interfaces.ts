//Enums-----------------------------------------------------------------------------------------------------

export enum Type{
	word=0,
	excel,
}

export enum Role{
	admin=0,
	user,
}

export enum Rights{
	"No rights"=0,
	"Read/Write"=6,
	Execute=1,
	All=7
}

//Functions-------------------------------------------------------------------------------------------------

export function getRoleName(role:number):string{
	return Role[role];
}

export function getTypeName(type:number):string{
	return Type[type];
}

export function getRightName(right:number):string{
	return Rights[right];
}

//Interfaces------------------------------------------------------------------------------------------------

export interface TemplateFile {
    id: number;
    name: string;
    path: string;
    localName: string;
	type: string;
	ownerId: number;
	groupId: number;
	ownerName: string;
	groupName: string;
	version: number;
}

export interface User {
    id: number;
    userName: string;
    nativeName: string;
    email: string;
	role: string;
	password: string;
}

export interface UserGroup {
    id: number;
    name: string;
    description: string;
    realGroup: string;
	memberNumber: string;
}

export interface UserGroupMember {
	id: number;
	user_id: number;
    userName: string;
	nativeName: string;
	rights: number;
}

export interface Group{
	id: number;
	groupName: string;
	description: string;
	latestVersion: number;
	defaultVersion: number;
	fileNumber: number;
	ownerId: number;
}

export interface GroupData{
	templates: TemplateFile[];
	group: Group;
}


export interface UserForm {
    id: number;
    userName: string;
    nativeName: string;
    email: string;
	role: string;
	password: string;
	repassword: string;
}

export interface GrouppedTemplates {
    id: number;
    groupName: string;
	description: string;
	ownerName: string;
    latestVersion: number;
	fileNumber: number;
	defaultVersion: number;
}