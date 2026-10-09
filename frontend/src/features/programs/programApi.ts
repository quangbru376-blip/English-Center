import type { Endpoint, HttpClient } from '../../lib/http';
import { object, requireWire, text, version, decimal, strings, references, capabilities, course, band } from '../../lib/academicWire';
import { completion, page } from '../../lib/m1Wire';
import { validId, validUuid } from '../administration/types';
export type ProgramSummary = {
    id: number;
    name: string;
    course: {
        id: number;
        code: string;
        name: string;
        status: string;
    };
    band: {
        id: number;
        code: string;
        minScore: string;
    };
    status: 'DRAFT' | 'ACTIVE' | 'INACTIVE';
    expectedLessonCount: number;
    isConfigurationFrozen: boolean;
    version: string;
};
export type ConfigurationManifest = {
    expectedVersions: {
        program: string;
        course: string;
        band: string;
        existingComponents: {
            id: number;
            version: string;
        }[];
    };
    expectedSet: {
        existingComponentIds: number[];
    };
};
export type ActivationManifest = {
    expectedVersions: {
        program: string;
        course: string;
        band: string;
        components: {
            id: number;
            version: string;
        }[];
    };
    expectedSet: {
        courseId: number;
        bandId: number;
        componentIds: number[];
    };
};
export type Validation = {
    totalWeight: string;
    finalExamCount: number;
    meetsActivationRules: boolean;
    issues: {
        field: string;
        message: string;
    }[];
};
export type Program = ProgramSummary & {
    components: {
        id: number;
        name: string;
        weight: string;
        kind: 'GENERAL' | 'LESSON' | 'FINAL_EXAM';
        version: string;
    }[];
    relationVersions: {
        course: string;
        band: string;
    };
    validation: Validation;
    allowedActions: string[];
    disabledReasons: Record<string, string>;
    configurationManifest: ConfigurationManifest | null;
    activationManifest: ActivationManifest | null;
};
export type ActivationPreview = {
    program: ProgramSummary;
    validation: Validation;
    eligible: boolean;
    blockedReasons: string[];
    manifest: ActivationManifest | null;
};
const base = ['id', 'name', 'course', 'band', 'status', 'expectedLessonCount', 'isConfigurationFrozen', 'version'];
function summary(r: Record<string, unknown>) { requireWire(validId(r.id) && text(r.name) && ['DRAFT', 'ACTIVE', 'INACTIVE'].includes(r.status as string) && validId(r.expectedLessonCount) && typeof r.isConfigurationFrozen === 'boolean' && version(r.version)); course(r.course); band(r.band); }
function validation(value: unknown) { const r = object(value, ['totalWeight', 'finalExamCount', 'meetsActivationRules', 'issues']); requireWire(decimal(r.totalWeight) && Number.isInteger(r.finalExamCount) && (r.finalExamCount as number) >= 0 && typeof r.meetsActivationRules === 'boolean' && Array.isArray(r.issues)); for (const item of r.issues) {
    const i = object(item, ['field', 'message']);
    requireWire(text(i.field) && text(i.message));
} }
function manifest(value: unknown, config: boolean) { if (value === null)
    return; const r = object(value, ['expectedVersions', 'expectedSet']); const v = object(r.expectedVersions, config ? ['program', 'course', 'band', 'existingComponents'] : ['program', 'course', 'band', 'components']); requireWire(version(v.program) && version(v.course) && version(v.band)); references(v[config ? 'existingComponents' : 'components']); const s = object(r.expectedSet, config ? ['existingComponentIds'] : ['courseId', 'bandId', 'componentIds']); if (!config)
    requireWire(validId(s.courseId) && validId(s.bandId)); const ids = s[config ? 'existingComponentIds' : 'componentIds']; requireWire(Array.isArray(ids) && ids.every(validId) && new Set(ids).size === ids.length); requireWire((v[config ? 'existingComponents' : 'components'] as {
    id: number;
}[]).map(x => x.id).sort((a, b) => a - b).join(',') === [...ids].sort((a, b) => a - b).join(',')); }
export function decodeProgram(value: unknown): Program { const r = object(value, [...base, 'components', 'relationVersions', 'validation', 'allowedActions', 'disabledReasons', 'configurationManifest', 'activationManifest']); summary(r); requireWire(Array.isArray(r.components)); const ids = new Set<number>(); for (const value of r.components) {
    const c = object(value, ['id', 'name', 'weight', 'kind', 'version']);
    requireWire(validId(c.id) && !ids.has(c.id) && text(c.name) && decimal(c.weight) && ['GENERAL', 'LESSON', 'FINAL_EXAM'].includes(c.kind as string) && version(c.version));
    ids.add(c.id);
} const rv = object(r.relationVersions, ['course', 'band']); requireWire(version(rv.course) && version(rv.band)); validation(r.validation); capabilities(r, ['EDIT_CONFIGURATION', 'RENAME', 'PREVIEW_ACTIVATION', 'ACTIVATE', 'RETURN_TO_DRAFT', 'DEACTIVATE']); manifest(r.configurationManifest, true); manifest(r.activationManifest, false); return r as Program; }
export const searchPrograms = (client: HttpClient, query: Record<string, string>, signal: AbortSignal) => client.request({ endpoint: 'programSearch', slot: 'program-search', query, signal, decode: v => page(v, x => { const r = object(x, base); summary(r); return r as ProgramSummary; }) });
export const loadProgram = (client: HttpClient, id: number, signal: AbortSignal) => client.request({ endpoint: 'programDetail', slot: 'program-detail', targetId: id, signal, decode: v => { const r = decodeProgram(v); requireWire(r.id === id); return r; } });
export const previewProgram = (client: HttpClient, id: number, signal: AbortSignal) => client.request({ endpoint: 'programPreview', slot: 'program-preview', targetId: id, signal, decode: v => { const r = object(v, ['program', 'validation', 'eligible', 'blockedReasons', 'manifest']); summary(object(r.program, base)); validation(r.validation); requireWire(typeof r.eligible === 'boolean' && strings(r.blockedReasons)); manifest(r.manifest, false); requireWire((r.program as ProgramSummary).id === id && r.eligible === (r.manifest !== null)); return r as ActivationPreview; } });
export const programCommand = (client: HttpClient, endpoint: Endpoint, body: unknown, requestId: string, signal: AbortSignal, targetId?: number) => client.request({ endpoint, body, requestId, signal, targetId, slot: 'program-command', decode: v => { const r = object(v, ['program', 'operationId']); requireWire(validUuid(r.operationId)); const program = decodeProgram(r.program); requireWire(targetId === undefined || program.id === targetId); return { program, operationId: r.operationId }; } });
export const programLookup = (client: HttpClient, action: string, requestId: string, signal: AbortSignal) => client.request({ endpoint: 'programResult', requestId, query: { action }, signal, slot: 'program-result', decode: v => completion(v, 'programId') });
