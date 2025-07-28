

import sys
import json
import numpy as np
from sklearn.cluster import KMeans


def main():
    try:
        received = input()
        parsed_data= json.loads(received)
        
        cords=[]
        for cord in parsed_data['positions']:
            cords.append([cord['x'],cord['y'],cord['z']])
        to_np = np.array(cords)
        kmeans = KMeans(n_clusters=3, random_state=0, n_init="auto").fit(to_np)
        
        sys.stdout.write(str(json.dumps({"peaks":kmeans.cluster_centers_.tolist()})))
    except Exception as err:
        sys.stdout.write(err)
    
main()