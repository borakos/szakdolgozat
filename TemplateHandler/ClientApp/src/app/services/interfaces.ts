//Enums-----------------------------------------------------------------------------------------------------

export enum Type{
	word=0,
	excel,
}

export enum Role{
	admin=0,
	user,
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
    latestVersion: number;
	fileNumber: number;
	usedVersion: number;
}