package org.hl7.tinkar.protobuf;


import com.google.protobuf.ByteString;
import com.google.protobuf.Timestamp;
import org.junit.jupiter.api.Test;

import java.time.LocalDateTime;

public class TestTinkarProtobuf {

    @Test
    public void testGetStamp(){

        //PublicId
        PBPublicId semanticId = PBPublicId.newBuilder().addId(ByteString.copyFromUtf8("semanticVersionId")).build();
        //StampChronology
        ///PublidId
        PBPublicId stampId = PBPublicId.newBuilder().addId(ByteString.copyFromUtf8("stampChronId")).build();
        ///STampVersion
        ////Status Concept
        PBConcept status = PBConcept.newBuilder().setPublicId(PBPublicId.newBuilder().addId(ByteString.copyFromUtf8("statusId")).build()).build();
        ////Time
        Timestamp time = Timestamp.newBuilder().setNanos(LocalDateTime.now().getNano()).build();
        ////Author Concept
        PBConcept author = PBConcept.newBuilder().setPublicId(PBPublicId.newBuilder().addId(ByteString.copyFromUtf8("authorId")).build()).build();
        ////Module Concept
        PBConcept module = PBConcept.newBuilder().setPublicId(PBPublicId.newBuilder().addId(ByteString.copyFromUtf8("moduleId")).build()).build();
        ////Path Concept
        PBConcept path = PBConcept.newBuilder().setPublicId(PBPublicId.newBuilder().addId(ByteString.copyFromUtf8("pathId")).build()).build();
        PBStampVersion stampVersion = PBStampVersion.newBuilder()
                .setStatus(status)
                .setTime(time)
                .setAuthor(author)
                .setPath(path)
                .build();
        PBStampChronology stampChronology = PBStampChronology.newBuilder()
                .setPublicId(stampId)
                .addStampVersions(stampVersion)
                .build();

        //Semantic Version
        PBSemanticVersion semanticVersion = PBSemanticVersion.newBuilder()
                .addFieldValues(PBField.newBuilder().setBoolValue(true).build())
                .setPublicId(semanticId)
                .setStamp(stampChronology)
                .build();

        PBSemanticChronology semanticChronology = PBSemanticChronology.newBuilder()
                .setPublicId(PBPublicId.newBuilder().addId(ByteString.copyFromUtf8("semanticChronId")).build())
                .setReferencedComponent(PBPublicId.newBuilder().addId(ByteString.copyFromUtf8("refId")).build())
                .setPatternForSemantic(PBPublicId.newBuilder().addId(ByteString.copyFromUtf8("patternId")).build())
                .addVersions(semanticVersion)
                .build();

        System.out.println(semanticChronology.getVersionsList().get(0).getStamp());

    }
}
